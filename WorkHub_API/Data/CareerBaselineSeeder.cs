using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkHub.API.Models;

namespace WorkHub.API.Data
{
    public static class CareerBaselineSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            // -------------------------------------------------
            // Load career roles and current baseline relations.
            // Existing role baselines are preserved.
            // Only roles with no CareerRoleSkills are auto-filled.
            // -------------------------------------------------

            var roles = await context.CareerRoles
                .Include(x => x.CareerCategory)
                .ToListAsync();

            var existingRoleSkills = await context.CareerRoleSkills
                .AsNoTracking()
                .ToListAsync();

            var rolesWithSkills = existingRoleSkills
                .Select(x => x.CareerRoleId)
                .ToHashSet();

            // Build a baseline for every active catalog role.
            var generated = new Dictionary<int, RoleBaseline>();

            foreach (var role in roles)
            {
                if (!role.IsActive || !role.CareerCategory.IsActive)
                {
                    continue;
                }

                var baseline = BuildBaseline(
                    role.CareerCategory.CategoryName,
                    role.RoleName);

                generated[role.CareerRoleId] = baseline;

                // Preserve manual/admin changes.
                if (!role.BaselineExperienceYears.HasValue)
                {
                    role.BaselineExperienceYears = baseline.ExperienceYears;
                }

                if (string.IsNullOrWhiteSpace(role.BaselineEducationRequirement))
                {
                    role.BaselineEducationRequirement = baseline.EducationRequirement;
                }
            }

            // -------------------------------------------------
            // Create any missing master skills first.
            // -------------------------------------------------

            var existingSkills = await context.Skills.ToListAsync();

            var skillLookup = existingSkills
                .GroupBy(x => x.SkillName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    x => x.Key,
                    x => x.First(),
                    StringComparer.OrdinalIgnoreCase);

            var requiredSkillDefinitions = generated.Values
                .SelectMany(x => x.Skills)
                .GroupBy(x => x.SkillName, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .ToList();

            foreach (var skillDefinition in requiredSkillDefinitions)
            {
                if (skillLookup.TryGetValue(skillDefinition.SkillName, out var existingSkill))
                {
                    if (string.IsNullOrWhiteSpace(existingSkill.Category))
                    {
                        existingSkill.Category = skillDefinition.Category;
                    }

                    continue;
                }

                var skill = new Skill
                {
                    SkillName = skillDefinition.SkillName,
                    Category = skillDefinition.Category,
                    IsActive = true
                };

                context.Skills.Add(skill);
                skillLookup[skillDefinition.SkillName] = skill;
            }

            await context.SaveChangesAsync();

            // Reload so every newly-created skill has a SkillId.
            var allSkills = await context.Skills.ToListAsync();

            skillLookup = allSkills
                .GroupBy(x => x.SkillName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    x => x.Key,
                    x => x.First(),
                    StringComparer.OrdinalIgnoreCase);

            // -------------------------------------------------
            // Add role-skill baselines only where that role
            // currently has no skill baseline at all.
            // This protects already curated roles such as
            // Software Engineer, Accountant, Civil Engineer, etc.
            // -------------------------------------------------

            foreach (var role in roles)
            {
                if (!generated.TryGetValue(role.CareerRoleId, out var baseline))
                {
                    continue;
                }

                if (rolesWithSkills.Contains(role.CareerRoleId))
                {
                    continue;
                }

                foreach (var skillDefinition in baseline.Skills)
                {
                    if (!skillLookup.TryGetValue(skillDefinition.SkillName, out var skill))
                    {
                        continue;
                    }

                    context.CareerRoleSkills.Add(
                        new CareerRoleSkill
                        {
                            CareerRoleId = role.CareerRoleId,
                            SkillId = skill.SkillId,
                            Importance = skillDefinition.Importance,
                            RecommendedYearsOfExperience = skillDefinition.RecommendedYears
                        });
                }
            }

            await context.SaveChangesAsync();
        }


        // =====================================================
        // BASELINE RESOLVER
        // =====================================================

        private static RoleBaseline BuildBaseline(
            string categoryName,
            string roleName)
        {
            return categoryName.Trim() switch
            {
                "Information Technology" => BuildInformationTechnology(roleName),
                "Accounting & Finance" => BuildAccounting(roleName),
                "Engineering" => BuildEngineering(roleName),
                "Healthcare" => BuildHealthcare(roleName),
                "Sales & Marketing" => BuildSalesMarketing(roleName),
                "Education" => BuildEducation(roleName),
                "Hospitality & Tourism" => BuildHospitality(roleName),
                "Construction & Trades" => BuildConstruction(roleName),
                "Human Resources" => BuildHumanResources(roleName),
                "Administration & Customer Service" => BuildAdministration(roleName),
                "Logistics & Supply Chain" => BuildLogistics(roleName),
                "Design & Creative" => BuildDesign(roleName),
                "Legal & Compliance" => BuildLegal(roleName),
                "Science & Laboratory" => BuildScience(roleName),
                "Community & Social Services" => BuildCommunity(roleName),
                "Retail" => BuildRetail(roleName),
                _ => GenericBaseline(roleName)
            };
        }


        // =====================================================
        // INFORMATION TECHNOLOGY
        // =====================================================

        private static RoleBaseline BuildInformationTechnology(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("data scientist"))
            {
                return B(
                    2m,
                    "Bachelor degree in Data Science, Computer Science, Statistics, Mathematics or a related field, or equivalent practical experience.",
                    S("Python", "Data & Analytics"),
                    S("SQL", "Data & Analytics"),
                    S("Statistics", "Data & Analytics"),
                    S("Machine Learning", "Data & Analytics"),
                    S("Data Analysis", "Data & Analytics"),
                    S("Data Visualization", "Data & Analytics"),
                    S("Pandas", "Data & Analytics", "Preferred"),
                    S("Git", "Software Development", "Preferred"));
            }

            if (role.Contains("data analyst"))
            {
                return B(
                    1m,
                    "Relevant qualification in Data Analytics, Information Technology, Business Analytics, Statistics or a related field, or equivalent practical experience.",
                    S("SQL", "Data & Analytics"),
                    S("Microsoft Excel", "Business"),
                    S("Data Analysis", "Data & Analytics"),
                    S("Data Visualization", "Data & Analytics"),
                    S("Statistics", "Data & Analytics"),
                    S("Power BI", "Data & Analytics", "Preferred"),
                    S("Python", "Data & Analytics", "Preferred"));
            }

            if (role.Contains("business analyst"))
            {
                return B(
                    1m,
                    "Relevant qualification in Business, Information Systems, Business Analytics or a related field, or equivalent practical experience.",
                    S("Business Analysis", "Business Analysis"),
                    S("Requirements Gathering", "Business Analysis"),
                    S("Process Mapping", "Business Analysis"),
                    S("Stakeholder Management", "Professional Skills"),
                    S("Microsoft Excel", "Business"),
                    S("Documentation", "Professional Skills"),
                    S("SQL", "Data & Analytics", "Preferred"),
                    S("Agile", "Software Development", "Preferred"));
            }

            if (role.Contains("qa") || role.Contains("quality assurance"))
            {
                return B(
                    1m,
                    "Relevant qualification in Information Technology, Software Engineering or a related field, or equivalent practical experience.",
                    S("Software Testing", "Software Development"),
                    S("Test Case Design", "Software Development"),
                    S("Bug Tracking", "Software Development"),
                    S("API Testing", "Software Development"),
                    S("Git", "Software Development"),
                    S("Test Automation", "Software Development", "Preferred"),
                    S("SQL", "Data & Analytics", "Preferred"));
            }

            if (role.Contains("database administrator"))
            {
                return B(
                    2m,
                    "Relevant qualification in Information Technology, Computer Science or Database Systems, or equivalent practical experience.",
                    S("SQL", "Database"),
                    S("Database Administration", "Database"),
                    S("Backup and Recovery", "Database"),
                    S("Database Security", "Database"),
                    S("Performance Tuning", "Database"),
                    S("Troubleshooting", "Information Technology"),
                    S("Cloud Databases", "Database", "Preferred"));
            }

            if (role.Contains("systems administrator"))
            {
                return B(
                    2m,
                    "Relevant qualification in Information Technology, Systems Administration or a related field, or equivalent practical experience.",
                    S("System Administration", "Information Technology"),
                    S("Windows Server", "Information Technology"),
                    S("Linux", "Information Technology"),
                    S("Active Directory", "Information Technology"),
                    S("Troubleshooting", "Information Technology"),
                    S("Networking Fundamentals", "Networking"),
                    S("PowerShell", "Information Technology", "Preferred"),
                    S("Cloud Computing", "Information Technology", "Preferred"));
            }

            if (role.Contains("support technician") || role.Contains("it support"))
            {
                return B(
                    1m,
                    "Relevant qualification in Information Technology or equivalent hands-on technical support experience.",
                    S("Technical Support", "Information Technology"),
                    S("Troubleshooting", "Information Technology"),
                    S("Windows", "Information Technology"),
                    S("Microsoft 365", "Information Technology"),
                    S("Hardware Support", "Information Technology"),
                    S("Customer Service", "Customer Service"),
                    S("Active Directory", "Information Technology", "Preferred"),
                    S("Networking Fundamentals", "Networking", "Preferred"));
            }

            if (role.Contains("network"))
            {
                return B(
                    2m,
                    "Relevant qualification in Networking, Information Technology or a related field, or equivalent practical experience.",
                    S("TCP/IP", "Networking"),
                    S("Routing and Switching", "Networking"),
                    S("Network Troubleshooting", "Networking"),
                    S("VLANs", "Networking"),
                    S("Network Security", "Networking"),
                    S("Cisco Networking", "Networking", "Preferred"),
                    S("Firewalls", "Networking", "Preferred"),
                    S("Network Automation", "Networking", "Preferred"));
            }

            if (role.Contains("cyber") || role.Contains("security engineer"))
            {
                return B(
                    2m,
                    "Relevant qualification in Cybersecurity, Information Technology or a related field, or equivalent practical experience.",
                    S("Cybersecurity Fundamentals", "Cybersecurity"),
                    S("Network Security", "Cybersecurity"),
                    S("Security Monitoring", "Cybersecurity"),
                    S("Incident Response", "Cybersecurity"),
                    S("Vulnerability Management", "Cybersecurity"),
                    S("SIEM", "Cybersecurity", "Preferred"),
                    S("Risk Assessment", "Cybersecurity", "Preferred"),
                    S("Identity and Access Management", "Cybersecurity", "Preferred"));
            }

            if (role.Contains("cloud") || role.Contains("devops"))
            {
                return B(
                    2m,
                    "Relevant qualification in Information Technology, Cloud Computing, Software Engineering or a related field, or equivalent practical experience.",
                    S("Cloud Computing", "Information Technology"),
                    S("Linux", "Information Technology"),
                    S("Git", "Software Development"),
                    S("CI/CD", "DevOps"),
                    S("Docker", "DevOps"),
                    S("Infrastructure as Code", "DevOps"),
                    S("Kubernetes", "DevOps", "Preferred"),
                    S("Cloud Security", "Cybersecurity", "Preferred"));
            }

            if (role.Contains("mobile"))
            {
                return B(
                    1m,
                    "Relevant qualification in Software Engineering, Computer Science or a related field, or equivalent practical experience.",
                    S("Programming Fundamentals", "Software Development"),
                    S("Mobile App Development", "Software Development"),
                    S("Git", "Software Development"),
                    S("API Development", "Software Development"),
                    S("Databases", "Software Development"),
                    S("Software Testing", "Software Development"),
                    S("Android Development", "Software Development", "Preferred"),
                    S("iOS Development", "Software Development", "Preferred"));
            }

            if (role.Contains("frontend"))
            {
                return B(
                    1m,
                    "Relevant qualification in Software Engineering, Web Development, Computer Science or a related field, or equivalent practical experience.",
                    S("HTML", "Web Development"),
                    S("CSS", "Web Development"),
                    S("JavaScript", "Web Development"),
                    S("Git", "Software Development"),
                    S("Responsive Web Design", "Web Development"),
                    S("API Integration", "Web Development"),
                    S("React", "Web Development", "Preferred"),
                    S("UI/UX Fundamentals", "Design", "Preferred"));
            }

            if (role.Contains("backend"))
            {
                return B(
                    1m,
                    "Relevant qualification in Software Engineering, Computer Science or a related field, or equivalent practical experience.",
                    S("Programming Fundamentals", "Software Development"),
                    S("API Development", "Software Development"),
                    S("Databases", "Software Development"),
                    S("SQL", "Data & Analytics"),
                    S("Git", "Software Development"),
                    S("Authentication and Authorization", "Software Development"),
                    S("Cloud Computing", "Information Technology", "Preferred"),
                    S("Docker", "DevOps", "Preferred"));
            }

            if (role.Contains("full stack"))
            {
                return B(
                    2m,
                    "Relevant qualification in Software Engineering, Computer Science or a related field, or equivalent practical experience.",
                    S("HTML", "Web Development"),
                    S("CSS", "Web Development"),
                    S("JavaScript", "Web Development"),
                    S("API Development", "Software Development"),
                    S("Databases", "Software Development"),
                    S("Git", "Software Development"),
                    S("Software Testing", "Software Development", "Preferred"),
                    S("Cloud Computing", "Information Technology", "Preferred"));
            }

            if (role.Contains("web developer"))
            {
                return B(
                    1m,
                    "Relevant qualification in Web Development, Software Engineering, Computer Science or a related field, or equivalent practical experience.",
                    S("HTML", "Web Development"),
                    S("CSS", "Web Development"),
                    S("JavaScript", "Web Development"),
                    S("Git", "Software Development"),
                    S("Responsive Web Design", "Web Development"),
                    S("Databases", "Software Development"),
                    S("API Development", "Software Development", "Preferred"),
                    S("Web Security", "Cybersecurity", "Preferred"));
            }

            // Software Engineer / Software Developer and general IT-dev fallback.
            return B(
                2m,
                "Bachelor degree in Computer Science, Software Engineering or a related field, or equivalent practical experience.",
                S("Programming Fundamentals", "Software Development"),
                S("Git", "Software Development"),
                S("Databases", "Software Development"),
                S("Software Development Lifecycle", "Software Development"),
                S("Problem Solving", "Professional Skills"),
                S("API Development", "Software Development", "Preferred"),
                S("Software Testing", "Software Development", "Preferred"),
                S("Cloud Computing", "Information Technology", "Preferred"));
        }


        // =====================================================
        // ACCOUNTING & FINANCE
        // =====================================================

        private static RoleBaseline BuildAccounting(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("bookkeeper"))
            {
                return B(1m,
                    "Relevant bookkeeping, accounting or finance qualification, or equivalent practical experience.",
                    S("Bookkeeping", "Accounting"),
                    S("Account Reconciliation", "Accounting"),
                    S("Accounts Payable", "Accounting"),
                    S("Accounts Receivable", "Accounting"),
                    S("Microsoft Excel", "Business"),
                    S("Xero", "Accounting Software", "Preferred"),
                    S("MYOB", "Accounting Software", "Preferred"));
            }

            if (role.Contains("financial analyst"))
            {
                return B(2m,
                    "Relevant qualification in Finance, Accounting, Economics, Business or a related field, or equivalent practical experience.",
                    S("Financial Analysis", "Finance"),
                    S("Financial Modelling", "Finance"),
                    S("Microsoft Excel", "Business"),
                    S("Financial Reporting", "Accounting"),
                    S("Data Analysis", "Data & Analytics"),
                    S("Budgeting", "Finance"),
                    S("Power BI", "Data & Analytics", "Preferred"));
            }

            if (role.Contains("payroll"))
            {
                return B(1m,
                    "Relevant payroll, accounting, finance or business qualification, or equivalent practical experience.",
                    S("Payroll Processing", "Accounting"),
                    S("Payroll Compliance", "Accounting"),
                    S("Microsoft Excel", "Business"),
                    S("Data Accuracy", "Professional Skills"),
                    S("Record Keeping", "Administration"),
                    S("Payroll Systems", "Accounting Software", "Preferred"));
            }

            if (role.Contains("accounts payable"))
            {
                return B(1m,
                    "Relevant accounting, finance or business qualification, or equivalent practical experience.",
                    S("Accounts Payable", "Accounting"),
                    S("Invoice Processing", "Accounting"),
                    S("Account Reconciliation", "Accounting"),
                    S("Microsoft Excel", "Business"),
                    S("Vendor Management", "Business"),
                    S("Accounting Software", "Accounting Software", "Preferred"));
            }

            if (role.Contains("accounts receivable"))
            {
                return B(1m,
                    "Relevant accounting, finance or business qualification, or equivalent practical experience.",
                    S("Accounts Receivable", "Accounting"),
                    S("Invoicing", "Accounting"),
                    S("Account Reconciliation", "Accounting"),
                    S("Microsoft Excel", "Business"),
                    S("Credit Control", "Accounting"),
                    S("Accounting Software", "Accounting Software", "Preferred"));
            }

            if (role.Contains("auditor"))
            {
                return B(2m,
                    "Relevant qualification in Accounting, Auditing, Finance or a related field, or equivalent professional experience.",
                    S("Auditing", "Accounting"),
                    S("Financial Reporting", "Accounting"),
                    S("Risk Assessment", "Finance"),
                    S("Internal Controls", "Accounting"),
                    S("Microsoft Excel", "Business"),
                    S("Compliance", "Legal & Compliance"),
                    S("Data Analysis", "Data & Analytics", "Preferred"));
            }

            if (role.Contains("credit"))
            {
                return B(1m,
                    "Relevant qualification in Finance, Accounting, Business or a related field, or equivalent practical experience.",
                    S("Credit Assessment", "Finance"),
                    S("Financial Analysis", "Finance"),
                    S("Risk Assessment", "Finance"),
                    S("Microsoft Excel", "Business"),
                    S("Customer Communication", "Customer Service"),
                    S("Record Keeping", "Administration"));
            }

            // Accountant / Assistant / Management / Tax / Finance Officer.
            var skills = new List<BaselineSkill>
            {
                S("Accounting Principles", "Accounting"),
                S("Financial Reporting", "Accounting"),
                S("Bookkeeping", "Accounting"),
                S("Microsoft Excel", "Business"),
                S("Account Reconciliation", "Accounting"),
                S("Xero", "Accounting Software", "Preferred"),
                S("MYOB", "Accounting Software", "Preferred")
            };

            if (role.Contains("tax"))
            {
                skills.Add(S("Tax Compliance", "Accounting"));
            }

            if (role.Contains("management accountant"))
            {
                skills.Add(S("Budgeting", "Finance"));
                skills.Add(S("Cost Accounting", "Accounting"));
            }

            return new RoleBaseline(
                2m,
                "Relevant accounting, finance or business qualification, or equivalent professional experience.",
                skills);
        }


        // =====================================================
        // ENGINEERING
        // =====================================================

        private static RoleBaseline BuildEngineering(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("mechanical"))
            {
                return B(2m,
                    "Relevant mechanical engineering qualification or equivalent practical experience.",
                    S("Mechanical Design", "Engineering"),
                    S("CAD", "Engineering Software"),
                    S("Engineering Drawings", "Engineering"),
                    S("Technical Analysis", "Engineering"),
                    S("Project Management", "Professional Skills"),
                    S("SolidWorks", "Engineering Software", "Preferred"),
                    S("Maintenance Engineering", "Engineering", "Preferred"));
            }

            if (role.Contains("electrical"))
            {
                return B(2m,
                    "Relevant electrical engineering qualification or equivalent practical experience.",
                    S("Electrical Engineering", "Engineering"),
                    S("Electrical Systems", "Electrical"),
                    S("Technical Drawings", "Engineering"),
                    S("Circuit Analysis", "Engineering"),
                    S("Project Management", "Professional Skills"),
                    S("AutoCAD", "Engineering Software", "Preferred"),
                    S("Testing and Inspection", "Electrical", "Preferred"));
            }

            if (role.Contains("electronics"))
            {
                return B(2m,
                    "Relevant electronics, electrical engineering or related qualification, or equivalent practical experience.",
                    S("Electronics", "Engineering"),
                    S("Circuit Analysis", "Engineering"),
                    S("PCB Design", "Engineering"),
                    S("Testing and Troubleshooting", "Engineering"),
                    S("Technical Documentation", "Engineering"),
                    S("Embedded Systems", "Engineering", "Preferred"));
            }

            if (role.Contains("chemical"))
            {
                return B(2m,
                    "Relevant chemical engineering qualification or equivalent practical experience.",
                    S("Chemical Engineering", "Engineering"),
                    S("Process Engineering", "Engineering"),
                    S("Process Safety", "Engineering"),
                    S("Data Analysis", "Data & Analytics"),
                    S("Quality Control", "Engineering"),
                    S("Project Management", "Professional Skills", "Preferred"));
            }

            if (role.Contains("industrial"))
            {
                return B(2m,
                    "Relevant industrial engineering, manufacturing or related qualification, or equivalent practical experience.",
                    S("Process Improvement", "Engineering"),
                    S("Lean Manufacturing", "Engineering"),
                    S("Data Analysis", "Data & Analytics"),
                    S("Quality Management", "Engineering"),
                    S("Project Management", "Professional Skills"),
                    S("Supply Chain Fundamentals", "Logistics", "Preferred"));
            }

            if (role.Contains("structural"))
            {
                return B(2m,
                    "Relevant civil or structural engineering qualification and professional requirements where applicable.",
                    S("Structural Analysis", "Engineering"),
                    S("Structural Design", "Engineering"),
                    S("AutoCAD", "Engineering Software"),
                    S("Technical Drawing", "Engineering"),
                    S("Construction Standards", "Engineering"),
                    S("Site Inspection", "Engineering", "Preferred"));
            }

            if (role.Contains("environmental"))
            {
                return B(2m,
                    "Relevant environmental engineering, environmental science or related qualification, or equivalent practical experience.",
                    S("Environmental Assessment", "Engineering"),
                    S("Environmental Compliance", "Engineering"),
                    S("Data Analysis", "Data & Analytics"),
                    S("Technical Reporting", "Engineering"),
                    S("Project Management", "Professional Skills"),
                    S("GIS", "Engineering Software", "Preferred"));
            }

            if (role.Contains("technician"))
            {
                return B(1m,
                    "Relevant engineering technician qualification, trade certificate or equivalent practical experience.",
                    S("Technical Drawing", "Engineering"),
                    S("Equipment Maintenance", "Engineering"),
                    S("Testing and Inspection", "Engineering"),
                    S("Troubleshooting", "Engineering"),
                    S("Safety Procedures", "Engineering"),
                    S("CAD", "Engineering Software", "Preferred"));
            }

            if (role.Contains("project engineer"))
            {
                return B(2m,
                    "Relevant engineering qualification and project delivery experience.",
                    S("Project Management", "Professional Skills"),
                    S("Engineering Design", "Engineering"),
                    S("Technical Documentation", "Engineering"),
                    S("Risk Management", "Professional Skills"),
                    S("Stakeholder Management", "Professional Skills"),
                    S("Cost Estimation", "Engineering", "Preferred"));
            }

            // Civil Engineer fallback.
            return B(2m,
                "Relevant civil engineering qualification and professional requirements where applicable.",
                S("Civil Engineering Design", "Engineering"),
                S("AutoCAD", "Engineering Software"),
                S("Structural Analysis", "Engineering"),
                S("Technical Drawing", "Engineering"),
                S("Construction Standards", "Engineering"),
                S("Project Management", "Professional Skills"),
                S("Cost Estimation", "Engineering", "Preferred"),
                S("Site Inspection", "Engineering", "Preferred"));
        }


        // =====================================================
        // HEALTHCARE
        // =====================================================

        private static RoleBaseline BuildHealthcare(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("registered nurse"))
            {
                return B(1m,
                    "Relevant nursing qualification and required professional registration or accreditation where applicable.",
                    S("Patient Care", "Healthcare"),
                    S("Clinical Assessment", "Healthcare"),
                    S("Medication Administration", "Healthcare"),
                    S("Clinical Documentation", "Healthcare"),
                    S("Infection Control", "Healthcare"),
                    S("Communication", "Professional Skills"),
                    S("Care Planning", "Healthcare", "Preferred"),
                    S("Emergency Response", "Healthcare", "Preferred"));
            }

            if (role.Contains("physiotherapist"))
            {
                return B(1m,
                    "Relevant physiotherapy qualification and professional registration where applicable.",
                    S("Patient Assessment", "Healthcare"),
                    S("Rehabilitation", "Healthcare"),
                    S("Treatment Planning", "Healthcare"),
                    S("Clinical Documentation", "Healthcare"),
                    S("Patient Education", "Healthcare"),
                    S("Communication", "Professional Skills"));
            }

            if (role.Contains("pharmacist"))
            {
                return B(1m,
                    "Relevant pharmacy qualification and professional registration where applicable.",
                    S("Medication Knowledge", "Healthcare"),
                    S("Dispensing", "Healthcare"),
                    S("Medication Safety", "Healthcare"),
                    S("Patient Counselling", "Healthcare"),
                    S("Clinical Documentation", "Healthcare"),
                    S("Inventory Management", "Healthcare", "Preferred"));
            }

            if (role.Contains("laboratory"))
            {
                return B(1m,
                    "Relevant medical laboratory, biomedical science or related qualification, or equivalent practical experience.",
                    S("Laboratory Procedures", "Laboratory"),
                    S("Sample Preparation", "Laboratory"),
                    S("Laboratory Safety", "Laboratory"),
                    S("Quality Control", "Laboratory"),
                    S("Data Recording", "Laboratory"),
                    S("Equipment Maintenance", "Laboratory", "Preferred"));
            }

            if (role.Contains("dental"))
            {
                return B(1m,
                    "Relevant dental assisting qualification or equivalent practical experience, with required checks or accreditation where applicable.",
                    S("Chairside Assistance", "Healthcare"),
                    S("Infection Control", "Healthcare"),
                    S("Sterilisation", "Healthcare"),
                    S("Patient Care", "Healthcare"),
                    S("Clinical Documentation", "Healthcare"),
                    S("Dental Instruments", "Healthcare", "Preferred"));
            }

            if (role.Contains("aged care") || role.Contains("disability support"))
            {
                return B(1m,
                    "Relevant care or community services qualification and required screening/checks where applicable.",
                    S("Personal Care", "Healthcare"),
                    S("Client Support", "Community Services"),
                    S("Care Planning", "Healthcare"),
                    S("Manual Handling", "Healthcare"),
                    S("Documentation", "Professional Skills"),
                    S("Communication", "Professional Skills"),
                    S("First Aid", "Healthcare", "Preferred"));
            }

            // Healthcare Administrator / Medical Administrator / Medical Receptionist.
            return B(1m,
                "Relevant healthcare administration, business or office qualification, or equivalent practical experience.",
                S("Healthcare Administration", "Healthcare"),
                S("Medical Terminology", "Healthcare"),
                S("Appointment Scheduling", "Administration"),
                S("Patient Records", "Healthcare"),
                S("Customer Service", "Customer Service"),
                S("Privacy and Confidentiality", "Healthcare"),
                S("Microsoft Office", "Business", "Preferred"));
        }


        // =====================================================
        // SALES & MARKETING
        // =====================================================

        private static RoleBaseline BuildSalesMarketing(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("digital marketing"))
            {
                return B(1m,
                    "Relevant marketing, communications or business qualification, or equivalent practical experience.",
                    S("Digital Marketing", "Marketing"),
                    S("SEO", "Marketing"),
                    S("Social Media Marketing", "Marketing"),
                    S("Content Marketing", "Marketing"),
                    S("Google Analytics", "Marketing"),
                    S("Campaign Management", "Marketing"),
                    S("Google Ads", "Marketing", "Preferred"),
                    S("Email Marketing", "Marketing", "Preferred"));
            }

            if (role.Contains("seo"))
            {
                return B(1m,
                    "Relevant marketing, digital media or equivalent practical experience.",
                    S("SEO", "Marketing"),
                    S("Keyword Research", "Marketing"),
                    S("Google Analytics", "Marketing"),
                    S("Content Marketing", "Marketing"),
                    S("Technical SEO", "Marketing"),
                    S("Google Search Console", "Marketing", "Preferred"));
            }

            if (role.Contains("social media"))
            {
                return B(1m,
                    "Relevant marketing, communications, digital media or equivalent practical experience.",
                    S("Social Media Marketing", "Marketing"),
                    S("Content Creation", "Marketing"),
                    S("Content Planning", "Marketing"),
                    S("Analytics", "Marketing"),
                    S("Community Management", "Marketing"),
                    S("Paid Social Advertising", "Marketing", "Preferred"));
            }

            if (role.Contains("content marketing"))
            {
                return B(1m,
                    "Relevant marketing, communications, journalism or equivalent practical experience.",
                    S("Content Marketing", "Marketing"),
                    S("Copywriting", "Marketing"),
                    S("SEO", "Marketing"),
                    S("Content Planning", "Marketing"),
                    S("Analytics", "Marketing"),
                    S("Email Marketing", "Marketing", "Preferred"));
            }

            if (role.Contains("marketing coordinator") || role.Contains("marketing specialist"))
            {
                return B(1m,
                    "Relevant marketing, communications or business qualification, or equivalent practical experience.",
                    S("Marketing", "Marketing"),
                    S("Campaign Management", "Marketing"),
                    S("Content Creation", "Marketing"),
                    S("Market Research", "Marketing"),
                    S("Communication", "Professional Skills"),
                    S("Microsoft Office", "Business"),
                    S("Digital Marketing", "Marketing", "Preferred"));
            }

            if (role.Contains("account manager"))
            {
                return B(2m,
                    "Relevant business, sales, marketing qualification or equivalent client-management experience.",
                    S("Account Management", "Sales"),
                    S("Client Relationship Management", "Sales"),
                    S("Sales", "Sales"),
                    S("Negotiation", "Professional Skills"),
                    S("Communication", "Professional Skills"),
                    S("CRM", "Sales", "Preferred"));
            }

            if (role.Contains("business development"))
            {
                return B(2m,
                    "Relevant business, sales or marketing qualification, or equivalent practical experience.",
                    S("Business Development", "Sales"),
                    S("Sales", "Sales"),
                    S("Lead Generation", "Sales"),
                    S("Negotiation", "Professional Skills"),
                    S("Client Relationship Management", "Sales"),
                    S("CRM", "Sales", "Preferred"));
            }

            // Sales Executive / Sales Representative.
            return B(1m,
                "Relevant sales, business or marketing qualification, or equivalent practical experience.",
                S("Sales", "Sales"),
                S("Customer Service", "Customer Service"),
                S("Lead Generation", "Sales"),
                S("Negotiation", "Professional Skills"),
                S("Communication", "Professional Skills"),
                S("Product Knowledge", "Sales"),
                S("CRM", "Sales", "Preferred"));
        }


        // =====================================================
        // EDUCATION
        // =====================================================

        private static RoleBaseline BuildEducation(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("early childhood"))
            {
                return B(1m,
                    "Relevant early childhood education qualification and required registration/checks where applicable.",
                    S("Early Childhood Education", "Education"),
                    S("Learning Activities", "Education"),
                    S("Child Development", "Education"),
                    S("Child Safety", "Education"),
                    S("Observation and Documentation", "Education"),
                    S("Communication", "Professional Skills"));
            }

            if (role.Contains("teaching assistant") || role.Contains("learning support"))
            {
                return B(1m,
                    "Relevant education support qualification or equivalent practical experience, with required checks where applicable.",
                    S("Student Support", "Education"),
                    S("Classroom Support", "Education"),
                    S("Learning Activities", "Education"),
                    S("Behaviour Support", "Education"),
                    S("Communication", "Professional Skills"),
                    S("Safeguarding", "Education"));
            }

            if (role.Contains("trainer") || role.Contains("lecturer") || role.Contains("tutor"))
            {
                return B(1m,
                    "Relevant subject qualification and teaching/training capability, with required accreditation where applicable.",
                    S("Training Delivery", "Education"),
                    S("Lesson Planning", "Education"),
                    S("Student Assessment", "Education"),
                    S("Learning Materials", "Education"),
                    S("Communication", "Professional Skills"),
                    S("Learning Management Systems", "Education Technology", "Preferred"));
            }

            if (role.Contains("administrator"))
            {
                return B(1m,
                    "Relevant education administration, business or office qualification, or equivalent practical experience.",
                    S("Education Administration", "Education"),
                    S("Student Records", "Education"),
                    S("Scheduling", "Administration"),
                    S("Microsoft Office", "Business"),
                    S("Customer Service", "Customer Service"),
                    S("Learning Management Systems", "Education Technology", "Preferred"));
            }

            return B(1m,
                "Relevant teaching qualification and required registration or accreditation where applicable.",
                S("Lesson Planning", "Education"),
                S("Classroom Management", "Education"),
                S("Curriculum Delivery", "Education"),
                S("Student Assessment", "Education"),
                S("Student Support", "Education"),
                S("Communication", "Professional Skills"),
                S("Learning Management Systems", "Education Technology", "Preferred"),
                S("Digital Literacy", "Professional Skills", "Preferred"));
        }


        // =====================================================
        // HOSPITALITY & TOURISM
        // =====================================================

        private static RoleBaseline BuildHospitality(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("chef") || role == "cook")
            {
                return B(2m,
                    "Relevant cookery or hospitality qualification, apprenticeship or equivalent practical experience.",
                    S("Food Preparation", "Hospitality"),
                    S("Kitchen Operations", "Hospitality"),
                    S("Food Safety", "Hospitality"),
                    S("Menu Planning", "Hospitality"),
                    S("Time Management", "Professional Skills"),
                    S("Inventory Control", "Hospitality", "Preferred"),
                    S("Cost Control", "Hospitality", "Preferred"));
            }

            if (role.Contains("barista"))
            {
                return B(0.5m,
                    "Hospitality training or equivalent practical experience is useful; formal qualification may not always be required.",
                    S("Coffee Preparation", "Hospitality"),
                    S("Customer Service", "Customer Service"),
                    S("POS Systems", "Retail"),
                    S("Food Safety", "Hospitality"),
                    S("Cash Handling", "Retail"),
                    S("Milk Texturing", "Hospitality", "Preferred"));
            }

            if (role.Contains("waitstaff"))
            {
                return B(0.5m,
                    "Hospitality or customer service experience is useful; formal qualification may not always be required.",
                    S("Customer Service", "Customer Service"),
                    S("Food and Beverage Service", "Hospitality"),
                    S("POS Systems", "Retail"),
                    S("Order Taking", "Hospitality"),
                    S("Communication", "Professional Skills"),
                    S("Food Safety", "Hospitality", "Preferred"));
            }

            if (role.Contains("kitchen hand"))
            {
                return B(0.5m,
                    "Hospitality experience is useful; formal qualification may not always be required.",
                    S("Kitchen Operations", "Hospitality"),
                    S("Food Safety", "Hospitality"),
                    S("Cleaning", "Hospitality"),
                    S("Food Preparation", "Hospitality"),
                    S("Time Management", "Professional Skills"));
            }

            if (role.Contains("restaurant manager") || role.Contains("hotel manager"))
            {
                return B(3m,
                    "Relevant hospitality, hotel management, business qualification or equivalent management experience.",
                    S("Hospitality Management", "Hospitality"),
                    S("Team Leadership", "Professional Skills"),
                    S("Customer Service", "Customer Service"),
                    S("Budgeting", "Finance"),
                    S("Operations Management", "Business"),
                    S("Staff Scheduling", "Hospitality"),
                    S("Inventory Control", "Hospitality", "Preferred"));
            }

            if (role.Contains("travel consultant"))
            {
                return B(1m,
                    "Relevant travel, tourism, hospitality qualification or equivalent practical experience.",
                    S("Travel Planning", "Tourism"),
                    S("Customer Service", "Customer Service"),
                    S("Booking Systems", "Tourism"),
                    S("Sales", "Sales"),
                    S("Communication", "Professional Skills"),
                    S("Destination Knowledge", "Tourism", "Preferred"));
            }

            // Front office / housekeeping supervisor fallback.
            return B(1m,
                "Relevant hospitality, tourism or customer-service qualification, or equivalent practical experience.",
                S("Customer Service", "Customer Service"),
                S("Hospitality Operations", "Hospitality"),
                S("Booking Systems", "Tourism"),
                S("Communication", "Professional Skills"),
                S("Problem Solving", "Professional Skills"),
                S("Team Coordination", "Professional Skills", "Preferred"));
        }


        // =====================================================
        // CONSTRUCTION & TRADES
        // =====================================================

        private static RoleBaseline BuildConstruction(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("project manager"))
            {
                return B(3m,
                    "Relevant construction management, engineering or building qualification, or equivalent project-management experience.",
                    S("Construction Management", "Construction"),
                    S("Project Management", "Professional Skills"),
                    S("Budgeting", "Finance"),
                    S("Scheduling", "Construction"),
                    S("Risk Management", "Professional Skills"),
                    S("Contract Administration", "Construction"),
                    S("Stakeholder Management", "Professional Skills"));
            }

            if (role.Contains("site supervisor"))
            {
                return B(2m,
                    "Relevant construction, trade or site-supervision qualification, licence or equivalent practical experience.",
                    S("Site Supervision", "Construction"),
                    S("Construction Safety", "Construction"),
                    S("Scheduling", "Construction"),
                    S("Quality Control", "Construction"),
                    S("Team Leadership", "Professional Skills"),
                    S("Construction Standards", "Engineering"));
            }

            if (role.Contains("quantity surveyor"))
            {
                return B(2m,
                    "Relevant quantity surveying, construction management or related qualification, or equivalent practical experience.",
                    S("Quantity Surveying", "Construction"),
                    S("Cost Estimation", "Engineering"),
                    S("Contract Administration", "Construction"),
                    S("Microsoft Excel", "Business"),
                    S("Construction Documentation", "Construction"),
                    S("Budgeting", "Finance"));
            }

            if (role.Contains("electrician"))
            {
                return B(2m,
                    "Relevant electrical trade qualification, apprenticeship and licence or registration where applicable.",
                    S("Electrical Systems", "Electrical"),
                    S("Electrical Safety", "Electrical"),
                    S("Fault Finding", "Electrical"),
                    S("Electrical Wiring", "Electrical"),
                    S("Technical Drawings", "Trades"),
                    S("Testing and Inspection", "Electrical"));
            }

            if (role.Contains("plumber"))
            {
                return B(2m,
                    "Relevant plumbing trade qualification, apprenticeship and licence or registration where applicable.",
                    S("Plumbing Systems", "Trades"),
                    S("Pipe Installation", "Trades"),
                    S("Fault Finding", "Trades"),
                    S("Safety Procedures", "Trades"),
                    S("Technical Drawings", "Trades"),
                    S("Maintenance", "Trades"));
            }

            if (role.Contains("carpenter"))
            {
                return B(2m,
                    "Relevant carpentry trade qualification, apprenticeship or equivalent practical experience.",
                    S("Carpentry", "Trades"),
                    S("Technical Drawings", "Trades"),
                    S("Measurement", "Trades"),
                    S("Power Tools", "Trades"),
                    S("Construction Safety", "Construction"),
                    S("Material Handling", "Trades"));
            }

            if (role.Contains("welder"))
            {
                return B(2m,
                    "Relevant welding trade qualification, certification or equivalent practical experience.",
                    S("Welding", "Trades"),
                    S("Metal Fabrication", "Trades"),
                    S("Technical Drawings", "Trades"),
                    S("Safety Procedures", "Trades"),
                    S("Quality Inspection", "Trades"));
            }

            if (role.Contains("painter"))
            {
                return B(1m,
                    "Relevant painting trade training or equivalent practical experience.",
                    S("Surface Preparation", "Trades"),
                    S("Painting", "Trades"),
                    S("Colour Matching", "Trades"),
                    S("Safety Procedures", "Trades"),
                    S("Material Handling", "Trades"));
            }

            if (role.Contains("bricklayer"))
            {
                return B(2m,
                    "Relevant bricklaying trade qualification, apprenticeship or equivalent practical experience.",
                    S("Bricklaying", "Trades"),
                    S("Measurement", "Trades"),
                    S("Construction Safety", "Construction"),
                    S("Technical Drawings", "Trades"),
                    S("Material Handling", "Trades"));
            }

            // Construction Labourer fallback.
            return B(0.5m,
                "Formal qualification may not always be required; construction experience and required safety credentials are useful.",
                S("Construction Safety", "Construction"),
                S("Manual Handling", "Construction"),
                S("Power Tools", "Trades"),
                S("Material Handling", "Trades"),
                S("Teamwork", "Professional Skills"));
        }


        // =====================================================
        // HUMAN RESOURCES
        // =====================================================

        private static RoleBaseline BuildHumanResources(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("recruit") || role.Contains("talent acquisition"))
            {
                return B(1m,
                    "Relevant Human Resources, Business or recruitment experience, or equivalent practical experience.",
                    S("Recruitment", "Human Resources"),
                    S("Candidate Screening", "Human Resources"),
                    S("Interview Coordination", "Human Resources"),
                    S("Stakeholder Management", "Professional Skills"),
                    S("Communication", "Professional Skills"),
                    S("Applicant Tracking Systems", "Human Resources", "Preferred"));
            }

            if (role.Contains("learning and development"))
            {
                return B(1m,
                    "Relevant Human Resources, Learning and Development, Education or Business qualification, or equivalent practical experience.",
                    S("Learning and Development", "Human Resources"),
                    S("Training Coordination", "Human Resources"),
                    S("Training Needs Analysis", "Human Resources"),
                    S("Communication", "Professional Skills"),
                    S("Learning Management Systems", "Education Technology", "Preferred"));
            }

            if (role.Contains("manager"))
            {
                return B(3m,
                    "Relevant Human Resources, Business qualification or equivalent senior HR experience.",
                    S("HR Strategy", "Human Resources"),
                    S("Employee Relations", "Human Resources"),
                    S("Performance Management", "Human Resources"),
                    S("HR Policies", "Human Resources"),
                    S("Team Leadership", "Professional Skills"),
                    S("Workplace Compliance", "Human Resources"),
                    S("HRIS", "Human Resources", "Preferred"));
            }

            return B(1m,
                "Relevant qualification in Human Resources, Business or a related field, or equivalent practical experience.",
                S("Recruitment", "Human Resources"),
                S("Employee Relations", "Human Resources"),
                S("HR Administration", "Human Resources"),
                S("HR Policies", "Human Resources"),
                S("Onboarding", "Human Resources"),
                S("Communication", "Professional Skills"),
                S("HRIS", "Human Resources", "Preferred"),
                S("Performance Management", "Human Resources", "Preferred"));
        }


        // =====================================================
        // ADMINISTRATION & CUSTOMER SERVICE
        // =====================================================

        private static RoleBaseline BuildAdministration(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("customer service") || role.Contains("call centre"))
            {
                return B(1m,
                    "Customer service, business or administration experience is useful; formal qualification may not always be required.",
                    S("Customer Service", "Customer Service"),
                    S("Communication", "Professional Skills"),
                    S("Problem Solving", "Professional Skills"),
                    S("CRM", "Customer Service"),
                    S("Data Entry", "Administration"),
                    S("Complaint Handling", "Customer Service", "Preferred"));
            }

            if (role.Contains("data entry"))
            {
                return B(0.5m,
                    "Administration or data-entry experience is useful; formal qualification may not always be required.",
                    S("Data Entry", "Administration"),
                    S("Data Accuracy", "Professional Skills"),
                    S("Microsoft Excel", "Business"),
                    S("Microsoft Office", "Business"),
                    S("Record Keeping", "Administration"));
            }

            if (role.Contains("executive assistant"))
            {
                return B(2m,
                    "Relevant business, administration qualification or equivalent executive-support experience.",
                    S("Calendar Management", "Administration"),
                    S("Meeting Coordination", "Administration"),
                    S("Document Management", "Administration"),
                    S("Microsoft Office", "Business"),
                    S("Communication", "Professional Skills"),
                    S("Confidentiality", "Professional Skills"),
                    S("Travel Coordination", "Administration", "Preferred"));
            }

            if (role.Contains("office manager"))
            {
                return B(2m,
                    "Relevant business, administration qualification or equivalent office-management experience.",
                    S("Office Administration", "Administration"),
                    S("Team Coordination", "Professional Skills"),
                    S("Microsoft Office", "Business"),
                    S("Vendor Management", "Business"),
                    S("Budgeting", "Finance"),
                    S("Document Management", "Administration"));
            }

            return B(1m,
                "Relevant administration, business or office qualification, or equivalent practical experience.",
                S("Office Administration", "Administration"),
                S("Microsoft Office", "Business"),
                S("Data Entry", "Administration"),
                S("Calendar Management", "Administration"),
                S("Document Management", "Administration"),
                S("Communication", "Professional Skills"),
                S("Customer Service", "Customer Service", "Preferred"));
        }


        // =====================================================
        // LOGISTICS & SUPPLY CHAIN
        // =====================================================

        private static RoleBaseline BuildLogistics(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("supply chain analyst"))
            {
                return B(2m,
                    "Relevant supply chain, logistics, business analytics or related qualification, or equivalent practical experience.",
                    S("Supply Chain Analysis", "Logistics"),
                    S("Microsoft Excel", "Business"),
                    S("Data Analysis", "Data & Analytics"),
                    S("Inventory Management", "Logistics"),
                    S("Demand Planning", "Logistics"),
                    S("ERP Systems", "Business Systems", "Preferred"),
                    S("Power BI", "Data & Analytics", "Preferred"));
            }

            if (role.Contains("procurement") || role.Contains("purchasing"))
            {
                return B(1m,
                    "Relevant procurement, supply chain, logistics, business qualification or equivalent practical experience.",
                    S("Procurement", "Logistics"),
                    S("Vendor Management", "Business"),
                    S("Negotiation", "Professional Skills"),
                    S("Purchase Orders", "Logistics"),
                    S("Microsoft Excel", "Business"),
                    S("ERP Systems", "Business Systems", "Preferred"));
            }

            if (role.Contains("inventory"))
            {
                return B(1m,
                    "Relevant logistics, warehousing, supply chain experience or equivalent practical experience.",
                    S("Inventory Management", "Logistics"),
                    S("Stock Control", "Logistics"),
                    S("Microsoft Excel", "Business"),
                    S("Data Accuracy", "Professional Skills"),
                    S("Warehouse Systems", "Logistics", "Preferred"));
            }

            if (role.Contains("warehouse") || role.Contains("storeperson"))
            {
                return B(1m,
                    "Warehousing or logistics experience is useful; relevant licences or tickets may be required for some roles.",
                    S("Warehouse Operations", "Logistics"),
                    S("Inventory Management", "Logistics"),
                    S("Picking and Packing", "Logistics"),
                    S("Manual Handling", "Construction"),
                    S("Workplace Safety", "Logistics"),
                    S("Forklift Operation", "Logistics", "Preferred"));
            }

            if (role.Contains("delivery driver"))
            {
                return B(1m,
                    "Relevant driving licence and delivery/logistics experience; additional requirements may depend on the vehicle and employer.",
                    S("Delivery Operations", "Logistics"),
                    S("Route Planning", "Logistics"),
                    S("Customer Service", "Customer Service"),
                    S("Time Management", "Professional Skills"),
                    S("Proof of Delivery", "Logistics"));
            }

            return B(1m,
                "Relevant qualification in logistics, supply chain, business or equivalent practical experience.",
                S("Logistics Coordination", "Logistics"),
                S("Inventory Management", "Logistics"),
                S("Shipment Tracking", "Logistics"),
                S("Supply Chain Fundamentals", "Logistics"),
                S("Microsoft Excel", "Business"),
                S("Vendor Coordination", "Logistics", "Preferred"),
                S("ERP Systems", "Business Systems", "Preferred"));
        }


        // =====================================================
        // DESIGN & CREATIVE
        // =====================================================

        private static RoleBaseline BuildDesign(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("ui designer") || role.Contains("ux/ui"))
            {
                return B(1m,
                    "Relevant UI/UX, interaction design, graphic design qualification or a strong portfolio demonstrating equivalent capability.",
                    S("UI Design", "Design"),
                    S("Figma", "Design Software"),
                    S("Wireframing", "Design"),
                    S("Prototyping", "Design"),
                    S("Design Systems", "Design"),
                    S("Responsive Design", "Design"),
                    S("User Research", "Design", "Preferred"));
            }

            if (role.Contains("ux designer"))
            {
                return B(1m,
                    "Relevant UX, interaction design, psychology, design qualification or a strong portfolio demonstrating equivalent capability.",
                    S("User Research", "Design"),
                    S("Wireframing", "Design"),
                    S("Prototyping", "Design"),
                    S("Usability Testing", "Design"),
                    S("Figma", "Design Software"),
                    S("Information Architecture", "Design"));
            }

            if (role.Contains("video editor"))
            {
                return B(1m,
                    "Relevant media, film, creative qualification or a strong portfolio demonstrating equivalent capability.",
                    S("Video Editing", "Creative"),
                    S("Adobe Premiere Pro", "Design Software"),
                    S("Storytelling", "Creative"),
                    S("Audio Editing", "Creative"),
                    S("Colour Correction", "Creative"),
                    S("After Effects", "Design Software", "Preferred"));
            }

            if (role.Contains("photographer"))
            {
                return B(1m,
                    "Relevant photography, media qualification or a strong portfolio demonstrating equivalent capability.",
                    S("Photography", "Creative"),
                    S("Photo Editing", "Creative"),
                    S("Adobe Photoshop", "Design Software"),
                    S("Lighting", "Creative"),
                    S("Composition", "Creative"),
                    S("Client Communication", "Professional Skills"));
            }

            if (role.Contains("animator") || role.Contains("motion"))
            {
                return B(1m,
                    "Relevant animation, motion design, multimedia qualification or a strong portfolio demonstrating equivalent capability.",
                    S("Animation", "Creative"),
                    S("Motion Graphics", "Creative"),
                    S("After Effects", "Design Software"),
                    S("Storyboarding", "Creative"),
                    S("Adobe Illustrator", "Design Software", "Preferred"),
                    S("3D Animation", "Creative", "Preferred"));
            }

            if (role.Contains("content creator"))
            {
                return B(1m,
                    "Relevant media, marketing, communications qualification or a strong portfolio demonstrating equivalent capability.",
                    S("Content Creation", "Marketing"),
                    S("Video Editing", "Creative"),
                    S("Social Media Marketing", "Marketing"),
                    S("Copywriting", "Marketing"),
                    S("Photography", "Creative", "Preferred"),
                    S("Analytics", "Marketing", "Preferred"));
            }

            return B(1m,
                "Relevant design qualification or a strong portfolio demonstrating equivalent practical capability.",
                S("Graphic Design", "Design"),
                S("Adobe Photoshop", "Design Software"),
                S("Adobe Illustrator", "Design Software"),
                S("Typography", "Design"),
                S("Layout Design", "Design"),
                S("Branding", "Design", "Preferred"),
                S("Figma", "Design Software", "Preferred"));
        }


        // =====================================================
        // LEGAL & COMPLIANCE
        // =====================================================

        private static RoleBaseline BuildLegal(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("compliance"))
            {
                return B(2m,
                    "Relevant legal, compliance, risk, business or finance qualification, or equivalent practical experience.",
                    S("Compliance", "Legal & Compliance"),
                    S("Regulatory Requirements", "Legal & Compliance"),
                    S("Risk Assessment", "Finance"),
                    S("Policy Review", "Legal & Compliance"),
                    S("Documentation", "Professional Skills"),
                    S("Audit Support", "Legal & Compliance", "Preferred"));
            }

            if (role.Contains("risk analyst"))
            {
                return B(2m,
                    "Relevant risk, finance, business, economics or related qualification, or equivalent practical experience.",
                    S("Risk Assessment", "Finance"),
                    S("Data Analysis", "Data & Analytics"),
                    S("Microsoft Excel", "Business"),
                    S("Compliance", "Legal & Compliance"),
                    S("Reporting", "Professional Skills"),
                    S("Risk Modelling", "Finance", "Preferred"));
            }

            if (role.Contains("contract administrator"))
            {
                return B(2m,
                    "Relevant legal, contract management, construction, business or related qualification, or equivalent practical experience.",
                    S("Contract Administration", "Legal & Compliance"),
                    S("Document Management", "Administration"),
                    S("Negotiation", "Professional Skills"),
                    S("Compliance", "Legal & Compliance"),
                    S("Stakeholder Management", "Professional Skills"));
            }

            if (role.Contains("governance"))
            {
                return B(2m,
                    "Relevant governance, legal, business or public administration qualification, or equivalent practical experience.",
                    S("Governance", "Legal & Compliance"),
                    S("Policy Review", "Legal & Compliance"),
                    S("Compliance", "Legal & Compliance"),
                    S("Risk Management", "Professional Skills"),
                    S("Reporting", "Professional Skills"));
            }

            // Legal Assistant / Paralegal.
            return B(1m,
                "Relevant legal studies, administration qualification or equivalent practical experience.",
                S("Legal Administration", "Legal"),
                S("Legal Research", "Legal"),
                S("Document Preparation", "Legal"),
                S("Case Management", "Legal"),
                S("Microsoft Office", "Business"),
                S("Client Communication", "Professional Skills"));
        }


        // =====================================================
        // SCIENCE & LABORATORY
        // =====================================================

        private static RoleBaseline BuildScience(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("research assistant"))
            {
                return B(1m,
                    "Relevant science, research, health, engineering or related qualification, depending on the research field.",
                    S("Research Methods", "Science"),
                    S("Data Collection", "Science"),
                    S("Data Analysis", "Data & Analytics"),
                    S("Literature Review", "Science"),
                    S("Technical Writing", "Science"),
                    S("Statistics", "Data & Analytics", "Preferred"));
            }

            if (role.Contains("environmental scientist"))
            {
                return B(2m,
                    "Relevant environmental science, environmental management or related qualification, or equivalent practical experience.",
                    S("Environmental Assessment", "Science"),
                    S("Field Sampling", "Science"),
                    S("Data Analysis", "Data & Analytics"),
                    S("Environmental Compliance", "Science"),
                    S("Technical Reporting", "Science"),
                    S("GIS", "Engineering Software", "Preferred"));
            }

            if (role.Contains("food technologist"))
            {
                return B(1m,
                    "Relevant food science, food technology or related qualification, or equivalent practical experience.",
                    S("Food Science", "Science"),
                    S("Food Safety", "Hospitality"),
                    S("Quality Control", "Laboratory"),
                    S("Product Development", "Science"),
                    S("Laboratory Procedures", "Laboratory"),
                    S("Regulatory Compliance", "Science"));
            }

            if (role.Contains("quality assurance"))
            {
                return B(1m,
                    "Relevant science, quality, manufacturing or related qualification, or equivalent practical experience.",
                    S("Quality Assurance", "Science"),
                    S("Quality Control", "Laboratory"),
                    S("Documentation", "Professional Skills"),
                    S("Compliance", "Legal & Compliance"),
                    S("Root Cause Analysis", "Professional Skills"),
                    S("Auditing", "Accounting", "Preferred"));
            }

            if (role.Contains("quality control"))
            {
                return B(1m,
                    "Relevant laboratory, science, quality or technical qualification, or equivalent practical experience.",
                    S("Quality Control", "Laboratory"),
                    S("Laboratory Procedures", "Laboratory"),
                    S("Testing", "Laboratory"),
                    S("Data Recording", "Laboratory"),
                    S("Safety Procedures", "Laboratory"));
            }

            return B(1m,
                "Relevant laboratory, science or technical qualification, or equivalent practical experience.",
                S("Laboratory Procedures", "Laboratory"),
                S("Sample Preparation", "Laboratory"),
                S("Laboratory Safety", "Laboratory"),
                S("Quality Control", "Laboratory"),
                S("Data Recording", "Laboratory"),
                S("Equipment Maintenance", "Laboratory", "Preferred"));
        }


        // =====================================================
        // COMMUNITY & SOCIAL SERVICES
        // =====================================================

        private static RoleBaseline BuildCommunity(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("youth worker"))
            {
                return B(1m,
                    "Relevant youth work, community services, social science qualification and required screening/checks where applicable.",
                    S("Youth Support", "Community Services"),
                    S("Case Management", "Community Services"),
                    S("Safeguarding", "Community Services"),
                    S("Client Assessment", "Community Services"),
                    S("Communication", "Professional Skills"),
                    S("Crisis Support", "Community Services", "Preferred"));
            }

            if (role.Contains("disability support"))
            {
                return B(1m,
                    "Relevant disability, community services or care qualification and required screening/checks where applicable.",
                    S("Disability Support", "Community Services"),
                    S("Personal Care", "Healthcare"),
                    S("Care Planning", "Community Services"),
                    S("Manual Handling", "Healthcare"),
                    S("Documentation", "Professional Skills"),
                    S("Communication", "Professional Skills"));
            }

            if (role.Contains("case manager"))
            {
                return B(2m,
                    "Relevant social work, community services, psychology or related qualification, or equivalent practical experience.",
                    S("Case Management", "Community Services"),
                    S("Client Assessment", "Community Services"),
                    S("Care Planning", "Community Services"),
                    S("Service Coordination", "Community Services"),
                    S("Documentation", "Professional Skills"),
                    S("Stakeholder Management", "Professional Skills"));
            }

            if (role.Contains("family support"))
            {
                return B(1m,
                    "Relevant community services, social work, family support qualification and required screening/checks where applicable.",
                    S("Family Support", "Community Services"),
                    S("Client Assessment", "Community Services"),
                    S("Care Planning", "Community Services"),
                    S("Safeguarding", "Community Services"),
                    S("Communication", "Professional Skills"));
            }

            return B(1m,
                "Relevant social work or community services qualification and required professional registration or screening where applicable.",
                S("Case Management", "Community Services"),
                S("Client Assessment", "Community Services"),
                S("Care Planning", "Community Services"),
                S("Safeguarding", "Community Services"),
                S("Community Services", "Community Services"),
                S("Documentation", "Professional Skills"),
                S("Communication", "Professional Skills"));
        }


        // =====================================================
        // RETAIL
        // =====================================================

        private static RoleBaseline BuildRetail(string roleName)
        {
            var role = roleName.ToLowerInvariant();

            if (role.Contains("store manager"))
            {
                return B(2m,
                    "Retail, business or management qualification is useful; equivalent retail-management experience may be suitable.",
                    S("Retail Management", "Retail"),
                    S("Team Leadership", "Professional Skills"),
                    S("Customer Service", "Retail"),
                    S("Sales", "Sales"),
                    S("Inventory Management", "Retail"),
                    S("Staff Scheduling", "Retail"),
                    S("Visual Merchandising", "Retail", "Preferred"));
            }

            if (role.Contains("supervisor") || role.Contains("assistant store manager"))
            {
                return B(1m,
                    "Retail experience is generally important; relevant business or retail qualification may be useful.",
                    S("Customer Service", "Retail"),
                    S("Retail Sales", "Retail"),
                    S("Team Coordination", "Professional Skills"),
                    S("POS Systems", "Retail"),
                    S("Inventory Management", "Retail"),
                    S("Merchandising", "Retail", "Preferred"));
            }

            if (role.Contains("merchandiser"))
            {
                return B(1m,
                    "Retail, merchandising, marketing experience or relevant qualification is useful.",
                    S("Merchandising", "Retail"),
                    S("Visual Merchandising", "Retail"),
                    S("Stock Management", "Retail"),
                    S("Product Presentation", "Retail"),
                    S("Retail Sales", "Retail", "Preferred"));
            }

            if (role.Contains("checkout"))
            {
                return B(0.5m,
                    "Formal qualification may not always be required; customer service and cash-handling experience are useful.",
                    S("POS Systems", "Retail"),
                    S("Cash Handling", "Retail"),
                    S("Customer Service", "Retail"),
                    S("Data Accuracy", "Professional Skills"),
                    S("Product Knowledge", "Retail", "Preferred"));
            }

            return B(0.5m,
                "Formal qualification may not always be required; relevant retail or customer service experience may be suitable.",
                S("Customer Service", "Retail"),
                S("Retail Sales", "Retail"),
                S("POS Systems", "Retail"),
                S("Cash Handling", "Retail"),
                S("Product Knowledge", "Retail"),
                S("Inventory Support", "Retail", "Preferred"),
                S("Merchandising", "Retail", "Preferred"));
        }


        // =====================================================
        // GENERIC FALLBACK
        // =====================================================

        private static RoleBaseline GenericBaseline(string roleName)
        {
            return B(1m,
                $"Relevant qualification or practical experience related to {roleName}.",
                S("Communication", "Professional Skills"),
                S("Problem Solving", "Professional Skills"),
                S("Teamwork", "Professional Skills"),
                S("Time Management", "Professional Skills"),
                S("Documentation", "Professional Skills", "Preferred"));
        }


        // =====================================================
        // HELPERS
        // =====================================================

        private static RoleBaseline B(
            decimal? years,
            string education,
            params BaselineSkill[] skills)
        {
            return new RoleBaseline(
                years,
                education,
                skills.ToList());
        }

        private static BaselineSkill S(
            string skillName,
            string category,
            string importance = "Required",
            decimal? recommendedYears = 1m)
        {
            return new BaselineSkill(
                skillName,
                category,
                importance,
                recommendedYears);
        }

        private sealed record RoleBaseline(
            decimal? ExperienceYears,
            string EducationRequirement,
            List<BaselineSkill> Skills);

        private sealed record BaselineSkill(
            string SkillName,
            string Category,
            string Importance,
            decimal? RecommendedYears);
    }
}

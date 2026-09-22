using Microsoft.EntityFrameworkCore;
using WorkHub.API.Models;

namespace WorkHub.API.Data
{
    public static class CareerDataSeeder
    {
        public static async Task SeedAsync(
            IServiceProvider services)
        {
            using var scope =
                services.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<
                        ApplicationDbContext>();


            // =========================================
            // CAREER CATEGORY + ROLE MASTER DATA
            // =========================================

            var seedData = new[]
            {
                new
                {
                    CategoryName =
                        "Information Technology",

                    Description =
                        "Software, networking, cybersecurity, cloud, data and IT support careers.",

                    Roles = new[]
                    {
                        "Software Engineer",
                        "Software Developer",
                        "Web Developer",
                        "Frontend Developer",
                        "Backend Developer",
                        "Full Stack Developer",
                        "Mobile App Developer",
                        "Network Engineer",
                        "Network Administrator",
                        "Cybersecurity Analyst",
                        "Security Engineer",
                        "Cloud Engineer",
                        "DevOps Engineer",
                        "IT Support Technician",
                        "Systems Administrator",
                        "Database Administrator",
                        "Data Analyst",
                        "Data Scientist",
                        "Business Analyst",
                        "QA Engineer"
                    }
                },


                new
                {
                    CategoryName =
                        "Accounting & Finance",

                    Description =
                        "Accounting, finance, payroll, auditing and financial administration careers.",

                    Roles = new[]
                    {
                        "Accountant",
                        "Assistant Accountant",
                        "Management Accountant",
                        "Tax Accountant",
                        "Bookkeeper",
                        "Financial Analyst",
                        "Finance Officer",
                        "Payroll Officer",
                        "Accounts Payable Officer",
                        "Accounts Receivable Officer",
                        "Auditor",
                        "Credit Officer"
                    }
                },


                new
                {
                    CategoryName =
                        "Engineering",

                    Description =
                        "Professional engineering, technical design and engineering project careers.",

                    Roles = new[]
                    {
                        "Civil Engineer",
                        "Mechanical Engineer",
                        "Electrical Engineer",
                        "Electronics Engineer",
                        "Chemical Engineer",
                        "Industrial Engineer",
                        "Project Engineer",
                        "Structural Engineer",
                        "Environmental Engineer",
                        "Engineering Technician"
                    }
                },


                new
                {
                    CategoryName =
                        "Healthcare",

                    Description =
                        "Healthcare, patient support, medical administration and allied health careers.",

                    Roles = new[]
                    {
                        "Registered Nurse",
                        "Healthcare Administrator",
                        "Medical Administrator",
                        "Medical Receptionist",
                        "Aged Care Worker",
                        "Disability Support Worker",
                        "Physiotherapist",
                        "Pharmacist",
                        "Medical Laboratory Technician",
                        "Dental Assistant"
                    }
                },


                new
                {
                    CategoryName =
                        "Sales & Marketing",

                    Description =
                        "Sales, business development, digital marketing and customer acquisition careers.",

                    Roles = new[]
                    {
                        "Sales Executive",
                        "Sales Representative",
                        "Business Development Executive",
                        "Business Development Manager",
                        "Marketing Coordinator",
                        "Marketing Specialist",
                        "Digital Marketing Specialist",
                        "SEO Specialist",
                        "Social Media Specialist",
                        "Content Marketing Specialist",
                        "Account Manager"
                    }
                },


                new
                {
                    CategoryName =
                        "Education",

                    Description =
                        "Teaching, training, early childhood and education administration careers.",

                    Roles = new[]
                    {
                        "Teacher",
                        "Primary School Teacher",
                        "Secondary School Teacher",
                        "Early Childhood Educator",
                        "Teaching Assistant",
                        "Trainer",
                        "Lecturer",
                        "Tutor",
                        "Education Administrator",
                        "Learning Support Officer"
                    }
                },


                new
                {
                    CategoryName =
                        "Hospitality & Tourism",

                    Description =
                        "Hospitality, food service, accommodation and tourism careers.",

                    Roles = new[]
                    {
                        "Chef",
                        "Cook",
                        "Kitchen Hand",
                        "Barista",
                        "Waitstaff",
                        "Restaurant Manager",
                        "Hotel Manager",
                        "Front Office Receptionist",
                        "Housekeeping Supervisor",
                        "Travel Consultant"
                    }
                },


                new
                {
                    CategoryName =
                        "Construction & Trades",

                    Description =
                        "Construction management, building and skilled trade careers.",

                    Roles = new[]
                    {
                        "Construction Project Manager",
                        "Site Supervisor",
                        "Quantity Surveyor",
                        "Carpenter",
                        "Electrician",
                        "Plumber",
                        "Painter",
                        "Welder",
                        "Bricklayer",
                        "Construction Labourer"
                    }
                },


                new
                {
                    CategoryName =
                        "Human Resources",

                    Description =
                        "Human resources, recruitment, talent and employee development careers.",

                    Roles = new[]
                    {
                        "HR Officer",
                        "HR Coordinator",
                        "HR Advisor",
                        "HR Manager",
                        "Recruiter",
                        "Recruitment Consultant",
                        "Talent Acquisition Specialist",
                        "Learning and Development Coordinator",
                        "People and Culture Coordinator"
                    }
                },


                new
                {
                    CategoryName =
                        "Administration & Customer Service",

                    Description =
                        "Office administration, customer support and general business support careers.",

                    Roles = new[]
                    {
                        "Administrative Assistant",
                        "Office Administrator",
                        "Receptionist",
                        "Executive Assistant",
                        "Customer Service Representative",
                        "Customer Service Officer",
                        "Data Entry Operator",
                        "Call Centre Representative",
                        "Office Manager"
                    }
                },


                new
                {
                    CategoryName =
                        "Logistics & Supply Chain",

                    Description =
                        "Warehousing, logistics, procurement, inventory and supply chain careers.",

                    Roles = new[]
                    {
                        "Logistics Coordinator",
                        "Supply Chain Coordinator",
                        "Supply Chain Analyst",
                        "Procurement Officer",
                        "Purchasing Officer",
                        "Inventory Controller",
                        "Warehouse Officer",
                        "Storeperson",
                        "Warehouse Supervisor",
                        "Delivery Driver"
                    }
                },


                new
                {
                    CategoryName =
                        "Design & Creative",

                    Description =
                        "Graphic design, digital design, media and creative production careers.",

                    Roles = new[]
                    {
                        "Graphic Designer",
                        "UI Designer",
                        "UX Designer",
                        "UX/UI Designer",
                        "Content Creator",
                        "Video Editor",
                        "Photographer",
                        "Animator",
                        "Motion Graphic Designer",
                        "Creative Designer"
                    }
                },


                new
                {
                    CategoryName =
                        "Legal & Compliance",

                    Description =
                        "Legal support, compliance, governance and risk careers.",

                    Roles = new[]
                    {
                        "Legal Assistant",
                        "Paralegal",
                        "Compliance Officer",
                        "Compliance Analyst",
                        "Risk Analyst",
                        "Contract Administrator",
                        "Governance Officer"
                    }
                },


                new
                {
                    CategoryName =
                        "Science & Laboratory",

                    Description =
                        "Laboratory, scientific research, environmental and quality careers.",

                    Roles = new[]
                    {
                        "Laboratory Technician",
                        "Laboratory Assistant",
                        "Research Assistant",
                        "Environmental Scientist",
                        "Food Technologist",
                        "Quality Assurance Officer",
                        "Quality Control Technician"
                    }
                },


                new
                {
                    CategoryName =
                        "Community & Social Services",

                    Description =
                        "Community support, social care and client support careers.",

                    Roles = new[]
                    {
                        "Social Worker",
                        "Community Support Worker",
                        "Disability Support Worker",
                        "Youth Worker",
                        "Case Manager",
                        "Family Support Worker",
                        "Community Services Officer"
                    }
                },


                new
                {
                    CategoryName =
                        "Retail",

                    Description =
                        "Retail sales, store operations and merchandising careers.",

                    Roles = new[]
                    {
                        "Retail Sales Assistant",
                        "Retail Sales Consultant",
                        "Store Manager",
                        "Assistant Store Manager",
                        "Retail Supervisor",
                        "Merchandiser",
                        "Checkout Operator"
                    }
                }
            };


            // =========================================
            // INSERT WITHOUT DUPLICATES
            // =========================================

            foreach (var categoryData in seedData)
            {
                var category =
                    await context.CareerCategories
                        .FirstOrDefaultAsync(
                            x =>
                                x.CategoryName ==
                                categoryData.CategoryName);


                // =====================================
                // CREATE CATEGORY
                // =====================================

                if (category == null)
                {
                    category =
                        new CareerCategory
                        {
                            CategoryName =
                                categoryData.CategoryName,

                            Description =
                                categoryData.Description,

                            IsActive =
                                true
                        };


                    context.CareerCategories.Add(
                        category);


                    await context.SaveChangesAsync();
                }


                // =====================================
                // CREATE MISSING ROLES
                // =====================================

                var existingRoleNames =
                    await context.CareerRoles
                        .Where(
                            x =>
                                x.CareerCategoryId ==
                                category.CareerCategoryId)
                        .Select(
                            x => x.RoleName)
                        .ToListAsync();


                foreach (var roleName
                         in categoryData.Roles)
                {
                    var alreadyExists =
                        existingRoleNames.Any(
                            existing =>
                                existing.Equals(
                                    roleName,
                                    StringComparison.OrdinalIgnoreCase));


                    if (alreadyExists)
                    {
                        continue;
                    }


                    context.CareerRoles.Add(
                        new CareerRole
                        {
                            CareerCategoryId =
                                category.CareerCategoryId,

                            RoleName =
                                roleName,

                            Description =
                                $"Career benchmark and job matching profile for {roleName}.",

                            IsActive =
                                true
                        });
                }


                await context.SaveChangesAsync();
            }
        }
    }
}
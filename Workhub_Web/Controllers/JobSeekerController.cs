using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    public class JobSeekerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private const long MaxCVFileSize =
            5 * 1024 * 1024;


        public JobSeekerController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =
                httpClientFactory;
        }


        // =========================================
        // JOB SEEKER APPLICATION
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Application()
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(token);


                // =================================
                // LOAD GENERAL USER PROFILE
                // =================================

                var userProfileResponse =
                    await client.GetAsync(
                        "api/UserProfile/me");


                if (!userProfileResponse
                    .IsSuccessStatusCode)
                {
                    TempData["JobSeekerError"] =
                        "Unable to load your personal profile.";

                    return RedirectToAction(
                        "Profile",
                        "Account");
                }


                var userProfile =
                    await userProfileResponse.Content
                        .ReadFromJsonAsync<
                            UserProfileApiResponse>();


                if (userProfile == null)
                {
                    TempData["JobSeekerError"] =
                        "Unable to load your profile information.";

                    return RedirectToAction(
                        "Profile",
                        "Account");
                }


                var model =
                    new JobSeekerApplicationViewModel
                    {
                        UserId =
                            userProfile.UserId,

                        FirstName =
                            userProfile.FirstName
                            ?? string.Empty,

                        LastName =
                            userProfile.LastName
                            ?? string.Empty,

                        Email =
                            userProfile.Email
                            ?? string.Empty,

                        DateOfBirth =
                            userProfile.DateOfBirth,

                        Country =
                            userProfile.Country,

                        Location =
                            userProfile.Location,

                        Phone =
                            userProfile.Phone,

                        HasProfileImage =
                            !string.IsNullOrWhiteSpace(
                                userProfile.ProfileImageUrl)
                    };


                // =================================
                // LOAD JOB SEEKER PROFILE
                // =================================

                var jobSeekerResponse =
                    await client.GetAsync(
                        "api/JobSeeker/profile");


                if (jobSeekerResponse
                    .IsSuccessStatusCode)
                {
                    var jobSeekerProfile =
                        await jobSeekerResponse.Content
                            .ReadFromJsonAsync<
                                JobSeekerProfileApiResponse>();


                    if (jobSeekerProfile != null)
                    {
                        model.JobSeekerProfileId =
                            jobSeekerProfile
                                .JobSeekerProfileId;

                        model.JobSeekerProfileExists =
                            true;

                        model.ProfessionalSummary =
                            jobSeekerProfile
                                .ProfessionalSummary;

                        model.PreferredJobCategory =
                            jobSeekerProfile
                                .PreferredJobCategory;

                        model.ExpectedSalary =
                            jobSeekerProfile
                                .ExpectedSalary;

                        model.CurrentLocation =
                            jobSeekerProfile
                                .CurrentLocation;
                    }
                }
                else if (
                    jobSeekerResponse.StatusCode !=
                    HttpStatusCode.NotFound)
                {
                    TempData["JobSeekerError"] =
                        "Unable to load your Job Seeker profile.";
                }


                // =================================
                // DEFAULT LOCATION
                // =================================

                if (string.IsNullOrWhiteSpace(
                        model.CurrentLocation))
                {
                    model.CurrentLocation =
                        model.Location;
                }


                // =================================
                // LOAD MASTER SKILLS
                // =================================

                var availableSkillsResponse =
                    await client.GetAsync(
                        "api/Skills");


                if (availableSkillsResponse
                    .IsSuccessStatusCode)
                {
                    var availableSkills =
                        await availableSkillsResponse
                            .Content
                            .ReadFromJsonAsync<
                                List<SkillApiResponse>>();


                    if (availableSkills != null)
                    {
                        model.AvailableSkills =
                            availableSkills
                                .Select(
                                    x =>
                                        new SkillOptionViewModel
                                        {
                                            SkillId =
                                                x.SkillId,

                                            SkillName =
                                                x.SkillName
                                                ?? string.Empty,

                                            Category =
                                                x.Category
                                        })
                                .ToList();
                    }
                }


                // =================================
                // LOAD JOB SEEKER DATA
                // =================================

                if (model.JobSeekerProfileExists)
                {
                    // =============================
                    // EDUCATION
                    // =============================

                    var educationResponse =
                        await client.GetAsync(
                            "api/jobseeker/education");


                    if (educationResponse
                        .IsSuccessStatusCode)
                    {
                        var education =
                            await educationResponse.Content
                                .ReadFromJsonAsync<
                                    List<EducationApiResponse>>();


                        if (education != null)
                        {
                            model.Educations =
                                education
                                    .Select(
                                        x =>
                                            new JobSeekerEducationItemViewModel
                                            {
                                                JobSeekerEducationId =
                                                    x.JobSeekerEducationId,

                                                InstitutionName =
                                                    x.InstitutionName
                                                    ?? string.Empty,

                                                Qualification =
                                                    x.Qualification
                                                    ?? string.Empty,

                                                FieldOfStudy =
                                                    x.FieldOfStudy,

                                                StartDate =
                                                    x.StartDate,

                                                EndDate =
                                                    x.EndDate,

                                                IsCurrentlyStudying =
                                                    x.IsCurrentlyStudying
                                            })
                                    .ToList();
                        }
                    }


                    // =============================
                    // SKILLS
                    // =============================

                    var skillsResponse =
                        await client.GetAsync(
                            "api/JobSeeker/skills");


                    if (skillsResponse
                        .IsSuccessStatusCode)
                    {
                        var skills =
                            await skillsResponse.Content
                                .ReadFromJsonAsync<
                                    List<JobSeekerSkillApiResponse>>();


                        if (skills != null)
                        {
                            model.Skills =
                                skills
                                    .Select(
                                        x =>
                                            new JobSeekerSkillItemViewModel
                                            {
                                                JobSeekerSkillId =
                                                    x.JobSeekerSkillId,

                                                SkillId =
                                                    x.SkillId,

                                                SkillName =
                                                    x.SkillName
                                                    ?? string.Empty,

                                                Category =
                                                    x.Category,

                                                ProficiencyLevel =
                                                    x.ProficiencyLevel
                                                    ?? string.Empty,

                                                YearsOfExperience =
                                                    x.YearsOfExperience
                                            })
                                    .ToList();
                        }
                    }


                    // =============================
                    // WORK EXPERIENCE
                    // =============================

                    var experienceResponse =
                        await client.GetAsync(
                            "api/jobseeker/experience");


                    if (experienceResponse
                        .IsSuccessStatusCode)
                    {
                        var experiences =
                            await experienceResponse.Content
                                .ReadFromJsonAsync<
                                    List<ExperienceApiResponse>>();


                        if (experiences != null)
                        {
                            model.Experiences =
                                experiences
                                    .Select(
                                        x =>
                                            new JobSeekerExperienceItemViewModel
                                            {
                                                JobSeekerExperienceId =
                                                    x.JobSeekerExperienceId,

                                                JobTitle =
                                                    x.JobTitle
                                                    ?? string.Empty,

                                                CompanyName =
                                                    x.CompanyName
                                                    ?? string.Empty,

                                                Location =
                                                    x.Location,

                                                StartDate =
                                                    x.StartDate,

                                                EndDate =
                                                    x.EndDate,

                                                IsCurrentJob =
                                                    x.IsCurrentJob,

                                                Description =
                                                    x.Description
                                            })
                                    .ToList();
                        }
                    }


                    // =============================
                    // CV DOCUMENTS
                    // =============================

                    var cvResponse =
                        await client.GetAsync(
                            "api/jobseeker/cv");


                    if (cvResponse
                        .IsSuccessStatusCode)
                    {
                        var cvs =
                            await cvResponse.Content
                                .ReadFromJsonAsync<
                                    List<CVApiResponse>>();


                        if (cvs != null)
                        {
                            model.CVDocuments =
                                cvs
                                    .Select(
                                        x =>
                                            new JobSeekerCVItemViewModel
                                            {
                                                CVDocumentId =
                                                    x.CVDocumentId,

                                                FileName =
                                                    x.FileName
                                                    ?? string.Empty,

                                                FileType =
                                                    x.FileType
                                                    ?? string.Empty,

                                                FileSize =
                                                    x.FileSize,

                                                IsPrimary =
                                                    x.IsPrimary,

                                                UploadedAt =
                                                    x.UploadedAt
                                            })
                                    .ToList();
                        }
                    }
                }


                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["JobSeekerError"] =
                    "Unable to connect to WorkHub API.";

                return RedirectToAction(
                    "Profile",
                    "Account");
            }
        }


        // =========================================
        // SAVE PROFESSIONAL PROFILE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            SaveProfessionalProfile(
                JobSeekerApplicationViewModel model)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (string.IsNullOrWhiteSpace(
                    model.PreferredJobCategory))
            {
                TempData["JobSeekerError"] =
                    "Please select your preferred job category.";

                return RedirectToAction(
                    "Application");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(token);


                var request =
                    new
                    {
                        professionalSummary =
                            Clean(
                                model.ProfessionalSummary),

                        preferredJobCategory =
                            Clean(
                                model.PreferredJobCategory),

                        expectedSalary =
                            model.ExpectedSalary,

                        currentLocation =
                            Clean(
                                model.CurrentLocation)
                    };


                var response =
                    await client.PutAsJsonAsync(
                        "api/JobSeeker/profile",
                        request);


                if (response.IsSuccessStatusCode)
                {
                    TempData["JobSeekerSuccess"] =
                        "Professional information saved successfully.";

                    return RedirectToAction(
                        "Application");
                }


                TempData["JobSeekerError"] =
                    await GetApiMessageAsync(
                        response);
            }
            catch (HttpRequestException)
            {
                TempData["JobSeekerError"] =
                    "Unable to save your Job Seeker profile.";
            }


            return RedirectToAction(
                "Application");
        }


        // =========================================
        // ADD EDUCATION
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            AddEducation(
                JobSeekerEducationFormViewModel model)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (string.IsNullOrWhiteSpace(
                    model.InstitutionName))
            {
                TempData["JobSeekerError"] =
                    "Institution name is required.";

                return RedirectToAction(
                    "Application");
            }


            if (string.IsNullOrWhiteSpace(
                    model.Qualification))
            {
                TempData["JobSeekerError"] =
                    "Qualification is required.";

                return RedirectToAction(
                    "Application");
            }


            if (!model.IsCurrentlyStudying &&
                model.StartDate.HasValue &&
                model.EndDate.HasValue &&
                model.EndDate.Value <
                model.StartDate.Value)
            {
                TempData["JobSeekerError"] =
                    "End date cannot be earlier than start date.";

                return RedirectToAction(
                    "Application");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(token);


                var request =
                    new
                    {
                        institutionName =
                            model.InstitutionName.Trim(),

                        qualification =
                            model.Qualification.Trim(),

                        fieldOfStudy =
                            Clean(
                                model.FieldOfStudy),

                        startDate =
                            model.StartDate,

                        endDate =
                            model.IsCurrentlyStudying
                                ? null
                                : model.EndDate,

                        isCurrentlyStudying =
                            model.IsCurrentlyStudying
                    };


                var response =
                    await client.PostAsJsonAsync(
                        "api/jobseeker/education",
                        request);


                if (response.IsSuccessStatusCode)
                {
                    TempData["JobSeekerSuccess"] =
                        "Education added successfully.";

                    return RedirectToAction(
                        "Application");
                }


                TempData["JobSeekerError"] =
                    await GetApiMessageAsync(
                        response);
            }
            catch (HttpRequestException)
            {
                TempData["JobSeekerError"] =
                    "Unable to add education.";
            }


            return RedirectToAction(
                "Application");
        }


        // =========================================
        // UPDATE EDUCATION
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            UpdateEducation(
                int id,
                JobSeekerEducationFormViewModel model)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (id <= 0)
            {
                TempData["JobSeekerError"] =
                    "Invalid education record.";

                return RedirectToAction(
                    "Application");
            }


            if (string.IsNullOrWhiteSpace(
                    model.InstitutionName) ||
                string.IsNullOrWhiteSpace(
                    model.Qualification))
            {
                TempData["JobSeekerError"] =
                    "Institution and qualification are required.";

                return RedirectToAction(
                    "Application");
            }


            if (!model.IsCurrentlyStudying &&
                model.StartDate.HasValue &&
                model.EndDate.HasValue &&
                model.EndDate.Value <
                model.StartDate.Value)
            {
                TempData["JobSeekerError"] =
                    "End date cannot be earlier than start date.";

                return RedirectToAction(
                    "Application");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(token);


                var request =
                    new
                    {
                        institutionName =
                            model.InstitutionName.Trim(),

                        qualification =
                            model.Qualification.Trim(),

                        fieldOfStudy =
                            Clean(
                                model.FieldOfStudy),

                        startDate =
                            model.StartDate,

                        endDate =
                            model.IsCurrentlyStudying
                                ? null
                                : model.EndDate,

                        isCurrentlyStudying =
                            model.IsCurrentlyStudying
                    };


                var response =
                    await client.PutAsJsonAsync(
                        $"api/jobseeker/education/{id}",
                        request);


                if (response.IsSuccessStatusCode)
                {
                    TempData["JobSeekerSuccess"] =
                        "Education updated successfully.";

                    return RedirectToAction(
                        "Application");
                }


                TempData["JobSeekerError"] =
                    await GetApiMessageAsync(
                        response);
            }
            catch (HttpRequestException)
            {
                TempData["JobSeekerError"] =
                    "Unable to update education.";
            }


            return RedirectToAction(
                "Application");
        }


        // =========================================
        // DELETE EDUCATION
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            DeleteEducation(
                int id)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (id <= 0)
            {
                TempData["JobSeekerError"] =
                    "Invalid education record.";

                return RedirectToAction(
                    "Application");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(token);


                var response =
                    await client.DeleteAsync(
                        $"api/jobseeker/education/{id}");


                if (response.IsSuccessStatusCode)
                {
                    TempData["JobSeekerSuccess"] =
                        "Education removed successfully.";
                }
                else
                {
                    TempData["JobSeekerError"] =
                        await GetApiMessageAsync(
                            response);
                }
            }
            catch (HttpRequestException)
            {
                TempData["JobSeekerError"] =
                    "Unable to remove education.";
            }


            return RedirectToAction(
                "Application");
        }


        // =========================================
        // ADD SKILL
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            AddSkill(
                JobSeekerSkillFormViewModel model)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (model.SkillId <= 0)
            {
                TempData["JobSeekerError"] =
                    "Please select a skill.";

                return RedirectToAction(
                    "Application");
            }


            if (string.IsNullOrWhiteSpace(
                    model.ProficiencyLevel))
            {
                TempData["JobSeekerError"] =
                    "Please select a proficiency level.";

                return RedirectToAction(
                    "Application");
            }


            if (model.YearsOfExperience < 0 ||
                model.YearsOfExperience > 50)
            {
                TempData["JobSeekerError"] =
                    "Years of experience must be between 0 and 50.";

                return RedirectToAction(
                    "Application");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(token);


                var request =
                    new
                    {
                        skillId =
                            model.SkillId,

                        proficiencyLevel =
                            model.ProficiencyLevel.Trim(),

                        yearsOfExperience =
                            model.YearsOfExperience
                    };


                var response =
                    await client.PostAsJsonAsync(
                        "api/JobSeeker/skills",
                        request);


                if (response.IsSuccessStatusCode)
                {
                    TempData["JobSeekerSuccess"] =
                        "Skill added successfully.";
                }
                else
                {
                    TempData["JobSeekerError"] =
                        await GetApiMessageAsync(
                            response);
                }
            }
            catch (HttpRequestException)
            {
                TempData["JobSeekerError"] =
                    "Unable to add skill.";
            }


            return RedirectToAction(
                "Application");
        }


        // =========================================
        // DELETE SKILL
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            DeleteSkill(
                int id)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (id <= 0)
            {
                TempData["JobSeekerError"] =
                    "Invalid skill record.";

                return RedirectToAction(
                    "Application");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(token);


                var response =
                    await client.DeleteAsync(
                        $"api/JobSeeker/skills/{id}");


                if (response.IsSuccessStatusCode)
                {
                    TempData["JobSeekerSuccess"] =
                        "Skill removed successfully.";
                }
                else
                {
                    TempData["JobSeekerError"] =
                        await GetApiMessageAsync(
                            response);
                }
            }
            catch (HttpRequestException)
            {
                TempData["JobSeekerError"] =
                    "Unable to remove skill.";
            }


            return RedirectToAction(
                "Application");
        }


        // =========================================
        // ADD WORK EXPERIENCE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            AddExperience(
                JobSeekerExperienceFormViewModel model)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (string.IsNullOrWhiteSpace(
                    model.JobTitle))
            {
                TempData["JobSeekerError"] =
                    "Job title is required.";

                return RedirectToAction(
                    "Application");
            }


            if (string.IsNullOrWhiteSpace(
                    model.CompanyName))
            {
                TempData["JobSeekerError"] =
                    "Company name is required.";

                return RedirectToAction(
                    "Application");
            }


            if (!model.StartDate.HasValue)
            {
                TempData["JobSeekerError"] =
                    "Start date is required.";

                return RedirectToAction(
                    "Application");
            }


            if (!model.IsCurrentJob &&
                !model.EndDate.HasValue)
            {
                TempData["JobSeekerError"] =
                    "End date is required if this is not your current job.";

                return RedirectToAction(
                    "Application");
            }


            if (!model.IsCurrentJob &&
                model.EndDate.HasValue &&
                model.EndDate.Value <
                model.StartDate.Value)
            {
                TempData["JobSeekerError"] =
                    "End date cannot be earlier than start date.";

                return RedirectToAction(
                    "Application");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(token);


                var request =
                    new
                    {
                        jobTitle =
                            model.JobTitle.Trim(),

                        companyName =
                            model.CompanyName.Trim(),

                        location =
                            Clean(
                                model.Location),

                        startDate =
                            model.StartDate.Value,

                        endDate =
                            model.IsCurrentJob
                                ? null
                                : model.EndDate,

                        isCurrentJob =
                            model.IsCurrentJob,

                        description =
                            Clean(
                                model.Description)
                    };


                var response =
                    await client.PostAsJsonAsync(
                        "api/jobseeker/experience",
                        request);


                if (response.IsSuccessStatusCode)
                {
                    TempData["JobSeekerSuccess"] =
                        "Work experience added successfully.";
                }
                else
                {
                    TempData["JobSeekerError"] =
                        await GetApiMessageAsync(
                            response);
                }
            }
            catch (HttpRequestException)
            {
                TempData["JobSeekerError"] =
                    "Unable to add work experience.";
            }


            return RedirectToAction(
                "Application");
        }


        // =========================================
        // UPDATE WORK EXPERIENCE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            UpdateExperience(
                int id,
                JobSeekerExperienceFormViewModel model)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (id <= 0)
            {
                TempData["JobSeekerError"] =
                    "Invalid work experience record.";

                return RedirectToAction(
                    "Application");
            }


            if (string.IsNullOrWhiteSpace(
                    model.JobTitle) ||
                string.IsNullOrWhiteSpace(
                    model.CompanyName))
            {
                TempData["JobSeekerError"] =
                    "Job title and company name are required.";

                return RedirectToAction(
                    "Application");
            }


            if (!model.StartDate.HasValue)
            {
                TempData["JobSeekerError"] =
                    "Start date is required.";

                return RedirectToAction(
                    "Application");
            }


            if (!model.IsCurrentJob &&
                !model.EndDate.HasValue)
            {
                TempData["JobSeekerError"] =
                    "End date is required if this is not your current job.";

                return RedirectToAction(
                    "Application");
            }


            if (!model.IsCurrentJob &&
                model.EndDate.HasValue &&
                model.EndDate.Value <
                model.StartDate.Value)
            {
                TempData["JobSeekerError"] =
                    "End date cannot be earlier than start date.";

                return RedirectToAction(
                    "Application");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(token);


                var request =
                    new
                    {
                        jobTitle =
                            model.JobTitle.Trim(),

                        companyName =
                            model.CompanyName.Trim(),

                        location =
                            Clean(
                                model.Location),

                        startDate =
                            model.StartDate.Value,

                        endDate =
                            model.IsCurrentJob
                                ? null
                                : model.EndDate,

                        isCurrentJob =
                            model.IsCurrentJob,

                        description =
                            Clean(
                                model.Description)
                    };


                var response =
                    await client.PutAsJsonAsync(
                        $"api/jobseeker/experience/{id}",
                        request);


                if (response.IsSuccessStatusCode)
                {
                    TempData["JobSeekerSuccess"] =
                        "Work experience updated successfully.";
                }
                else
                {
                    TempData["JobSeekerError"] =
                        await GetApiMessageAsync(
                            response);
                }
            }
            catch (HttpRequestException)
            {
                TempData["JobSeekerError"] =
                    "Unable to update work experience.";
            }


            return RedirectToAction(
                "Application");
        }


        // =========================================
        // DELETE WORK EXPERIENCE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            DeleteExperience(
                int id)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (id <= 0)
            {
                TempData["JobSeekerError"] =
                    "Invalid work experience record.";

                return RedirectToAction(
                    "Application");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(token);


                var response =
                    await client.DeleteAsync(
                        $"api/jobseeker/experience/{id}");


                if (response.IsSuccessStatusCode)
                {
                    TempData["JobSeekerSuccess"] =
                        "Work experience removed successfully.";
                }
                else
                {
                    TempData["JobSeekerError"] =
                        await GetApiMessageAsync(
                            response);
                }
            }
            catch (HttpRequestException)
            {
                TempData["JobSeekerError"] =
                    "Unable to remove work experience.";
            }


            return RedirectToAction(
                "Application");
        }


        // =========================================
        // UPLOAD CV
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCV(
            IFormFile? file,
            bool isPrimary = true)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // =====================================
            // FILE REQUIRED
            // =====================================

            if (file == null ||
                file.Length == 0)
            {
                TempData["JobSeekerError"] =
                    "Please select a CV file.";

                return RedirectToAction(
                    "Application");
            }


            // =====================================
            // FILE SIZE
            // =====================================

            if (file.Length > MaxCVFileSize)
            {
                TempData["JobSeekerError"] =
                    "CV file size cannot exceed 5 MB.";

                return RedirectToAction(
                    "Application");
            }


            // =====================================
            // EXTENSION
            // =====================================

            var extension =
                Path.GetExtension(
                    file.FileName)
                    .ToLowerInvariant();


            if (extension != ".pdf" &&
                extension != ".docx")
            {
                TempData["JobSeekerError"] =
                    "Only PDF and DOCX CV files are allowed.";

                return RedirectToAction(
                    "Application");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(token);


                using var form =
                    new MultipartFormDataContent();


                // =================================
                // FILE CONTENT
                // =================================

                await using var stream =
                    file.OpenReadStream();


                using var fileContent =
                    new StreamContent(stream);


                if (!string.IsNullOrWhiteSpace(
                        file.ContentType))
                {
                    fileContent.Headers.ContentType =
                        new MediaTypeHeaderValue(
                            file.ContentType);
                }
                else
                {
                    fileContent.Headers.ContentType =
                        new MediaTypeHeaderValue(
                            extension == ".pdf"
                                ? "application/pdf"
                                : "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                }


                form.Add(
                    fileContent,
                    "File",
                    Path.GetFileName(
                        file.FileName));


                // =================================
                // PRIMARY VALUE
                // =================================

                form.Add(
                    new StringContent(
                        isPrimary.ToString()
                            .ToLowerInvariant()),
                    "IsPrimary");


                // =================================
                // SEND TO API
                // =================================

                var response =
                    await client.PostAsync(
                        "api/jobseeker/cv/upload",
                        form);


                if (response.IsSuccessStatusCode)
                {
                    TempData["JobSeekerSuccess"] =
                        "CV uploaded successfully.";
                }
                else
                {
                    TempData["JobSeekerError"] =
                        await GetApiMessageAsync(
                            response);
                }
            }
            catch (HttpRequestException)
            {
                TempData["JobSeekerError"] =
                    "Unable to upload CV.";
            }


            return RedirectToAction(
                "Application");
        }


        // =========================================
        // SET PRIMARY CV
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrimaryCV(
            int id)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (id <= 0)
            {
                TempData["JobSeekerError"] =
                    "Invalid CV.";

                return RedirectToAction(
                    "Application");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(token);


                using var request =
                    new HttpRequestMessage(
                        HttpMethod.Put,
                        $"api/jobseeker/cv/{id}/primary");


                var response =
                    await client.SendAsync(
                        request);


                if (response.IsSuccessStatusCode)
                {
                    TempData["JobSeekerSuccess"] =
                        "Primary CV updated successfully.";
                }
                else
                {
                    TempData["JobSeekerError"] =
                        await GetApiMessageAsync(
                            response);
                }
            }
            catch (HttpRequestException)
            {
                TempData["JobSeekerError"] =
                    "Unable to update primary CV.";
            }


            return RedirectToAction(
                "Application");
        }


        // =========================================
        // DOWNLOAD CV
        // =========================================

        [HttpGet]
        public async Task<IActionResult> DownloadCV(
            int id)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (id <= 0)
            {
                TempData["JobSeekerError"] =
                    "Invalid CV.";

                return RedirectToAction(
                    "Application");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(token);


                var response =
                    await client.GetAsync(
                        $"api/jobseeker/cv/{id}/download");


                if (!response.IsSuccessStatusCode)
                {
                    TempData["JobSeekerError"] =
                        await GetApiMessageAsync(
                            response);

                    return RedirectToAction(
                        "Application");
                }


                var fileBytes =
                    await response.Content
                        .ReadAsByteArrayAsync();


                var contentType =
                    response.Content.Headers.ContentType?
                        .MediaType
                    ?? "application/octet-stream";


                var fileName =
                    response.Content.Headers
                        .ContentDisposition?
                        .FileNameStar;


                if (string.IsNullOrWhiteSpace(
                        fileName))
                {
                    fileName =
                        response.Content.Headers
                            .ContentDisposition?
                            .FileName;
                }


                fileName =
                    fileName?
                        .Trim('"');


                if (string.IsNullOrWhiteSpace(
                        fileName))
                {
                    fileName =
                        $"WorkHub-CV-{id}";
                }


                return File(
                    fileBytes,
                    contentType,
                    fileName);
            }
            catch (HttpRequestException)
            {
                TempData["JobSeekerError"] =
                    "Unable to download CV.";

                return RedirectToAction(
                    "Application");
            }
        }


        // =========================================
        // DELETE CV
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCV(
            int id)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (id <= 0)
            {
                TempData["JobSeekerError"] =
                    "Invalid CV.";

                return RedirectToAction(
                    "Application");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(token);


                var response =
                    await client.DeleteAsync(
                        $"api/jobseeker/cv/{id}");


                if (response.IsSuccessStatusCode)
                {
                    TempData["JobSeekerSuccess"] =
                        "CV deleted successfully.";
                }
                else
                {
                    TempData["JobSeekerError"] =
                        await GetApiMessageAsync(
                            response);
                }
            }
            catch (HttpRequestException)
            {
                TempData["JobSeekerError"] =
                    "Unable to delete CV.";
            }


            return RedirectToAction(
                "Application");
        }


        // =========================================
        // AUTHENTICATED HTTP CLIENT
        // =========================================

        private HttpClient
            CreateAuthenticatedClient(
                string token)
        {
            var client =
                _httpClientFactory.CreateClient(
                    "WorkHubApi");


            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);


            return client;
        }


        // =========================================
        // CLEAN STRING
        // =========================================

        private static string? Clean(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                    value)
                ? null
                : value.Trim();
        }


        // =========================================
        // READ API MESSAGE
        // =========================================

        private static async Task<string>
            GetApiMessageAsync(
                HttpResponseMessage response)
        {
            try
            {
                var text =
                    await response.Content
                        .ReadAsStringAsync();


                if (string.IsNullOrWhiteSpace(text))
                {
                    return
                        $"Request failed. Status {(int)response.StatusCode}.";
                }


                using var document =
                    JsonDocument.Parse(text);


                if (document.RootElement
                    .TryGetProperty(
                        "message",
                        out var message))
                {
                    return message.GetString()
                        ?? "Request failed.";
                }


                if (document.RootElement
                    .TryGetProperty(
                        "title",
                        out var title))
                {
                    return title.GetString()
                        ?? "Request failed.";
                }
            }
            catch
            {
            }


            return
                $"Request failed. Status {(int)response.StatusCode}.";
        }


        // =========================================
        // USER PROFILE API RESPONSE
        // =========================================

        private class UserProfileApiResponse
        {
            public int UserId { get; set; }

            public string? FirstName { get; set; }

            public string? LastName { get; set; }

            public string? Email { get; set; }

            public DateTime? DateOfBirth { get; set; }

            public string? Country { get; set; }

            public string? Location { get; set; }

            public string? Phone { get; set; }

            public string? ProfileImageUrl { get; set; }
        }


        // =========================================
        // JOB SEEKER PROFILE API RESPONSE
        // =========================================

        private class JobSeekerProfileApiResponse
        {
            public int JobSeekerProfileId { get; set; }

            public int UserId { get; set; }

            public string? ProfessionalSummary { get; set; }

            public string? PreferredJobCategory { get; set; }

            public decimal? ExpectedSalary { get; set; }

            public string? CurrentLocation { get; set; }

            public bool IsActive { get; set; }

            public DateTime CreatedAt { get; set; }
        }


        // =========================================
        // EDUCATION API RESPONSE
        // =========================================

        private class EducationApiResponse
        {
            public int JobSeekerEducationId { get; set; }

            public string? InstitutionName { get; set; }

            public string? Qualification { get; set; }

            public string? FieldOfStudy { get; set; }

            public DateTime? StartDate { get; set; }

            public DateTime? EndDate { get; set; }

            public bool IsCurrentlyStudying { get; set; }
        }


        // =========================================
        // MASTER SKILL API RESPONSE
        // =========================================

        private class SkillApiResponse
        {
            public int SkillId { get; set; }

            public string? SkillName { get; set; }

            public string? Category { get; set; }
        }


        // =========================================
        // JOB SEEKER SKILL API RESPONSE
        // =========================================

        private class JobSeekerSkillApiResponse
        {
            public int JobSeekerSkillId { get; set; }

            public int SkillId { get; set; }

            public string? SkillName { get; set; }

            public string? Category { get; set; }

            public string? ProficiencyLevel { get; set; }

            public decimal YearsOfExperience { get; set; }
        }


        // =========================================
        // WORK EXPERIENCE API RESPONSE
        // =========================================

        private class ExperienceApiResponse
        {
            public int JobSeekerExperienceId { get; set; }

            public string? JobTitle { get; set; }

            public string? CompanyName { get; set; }

            public string? Location { get; set; }

            public DateTime StartDate { get; set; }

            public DateTime? EndDate { get; set; }

            public bool IsCurrentJob { get; set; }

            public string? Description { get; set; }
        }


        // =========================================
        // CV API RESPONSE
        // =========================================

        private class CVApiResponse
        {
            public int CVDocumentId { get; set; }

            public string? FileName { get; set; }

            public string? FileType { get; set; }

            public long FileSize { get; set; }

            public bool IsPrimary { get; set; }

            public DateTime UploadedAt { get; set; }
        }
    }
}
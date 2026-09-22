using Microsoft.EntityFrameworkCore;
using System.Linq;
using WorkHub.API.Models;

namespace WorkHub.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserNotification>
            UserNotifications
        { get; set; }
        // =========================================
        // USER
        // =========================================

        public DbSet<User> Users { get; set; }

        public DbSet<UserPublicProfile>
            UserPublicProfiles
        { get; set; }

        public DbSet<UserPublicProfileSkill>
            UserPublicProfileSkills
        { get; set; }

        public DbSet<UserPublicProfileEducation>
            UserPublicProfileEducations
        { get; set; }


        // =========================================
        // JOB SEEKER
        // =========================================

        public DbSet<JobSeekerProfile>
            JobSeekerProfiles
        { get; set; }

        public DbSet<JobSeekerSkill>
            JobSeekerSkills
        { get; set; }

        public DbSet<JobSeekerExperience>
            JobSeekerExperiences
        { get; set; }

        public DbSet<JobSeekerEducation>
            JobSeekerEducations
        { get; set; }

        public DbSet<CVDocument>
            CVDocuments
        { get; set; }


        // =========================================
        // CAREER INTELLIGENCE
        // =========================================

        public DbSet<CareerCategory>
            CareerCategories
        { get; set; }

        public DbSet<CareerRole>
            CareerRoles
        { get; set; }

        public DbSet<CareerRoleSkill>
            CareerRoleSkills
        { get; set; }


        // =========================================
        // COMPANY / JOBS
        // =========================================

        public DbSet<CompanyProfile>
            CompanyProfiles
        { get; set; }

        public DbSet<Job>
            Jobs
        { get; set; }

        public DbSet<Skill>
            Skills
        { get; set; }

        public DbSet<JobSkill>
            JobSkills
        { get; set; }

        public DbSet<JobApplication>
            JobApplications
        { get; set; }

        public DbSet<CVMatchResult>
            CVMatchResults
        { get; set; }

        public DbSet<JobScreeningQuestion>
            JobScreeningQuestions
        { get; set; }

        public DbSet<JobScreeningAnswer>
            JobScreeningAnswers
        { get; set; }

        public DbSet<CVAnalysisPurchase>
            CVAnalysisPurchases
        { get; set; }


        // =========================================
        // SERVICE PROVIDER
        // =========================================

        public DbSet<ServiceProviderProfile>
            ServiceProviderProfiles
        { get; set; }

        public DbSet<ServiceCategory>
            ServiceCategories
        { get; set; }

        public DbSet<ServiceOffering>
            ServiceOfferings
        { get; set; }

        public DbSet<ServiceRequest>
            ServiceRequests
        { get; set; }

        public DbSet<ServiceOffer>
            ServiceOffers
        { get; set; }

        public DbSet<ServiceReview>
            ServiceReviews
        { get; set; }

        public DbSet<NewsUpdate> NewsUpdates { get; set; }
        // =========================================
        // MARKETPLACE
        // =========================================

        public DbSet<MarketplaceSellerProfile>
            MarketplaceSellerProfiles
        { get; set; }

        public DbSet<MarketplaceCategory>
            MarketplaceCategories
        { get; set; }

        public DbSet<MarketplaceListing>
            MarketplaceListings
        { get; set; }

        public DbSet<MarketplaceImage>
            MarketplaceImages
        { get; set; }

        public DbSet<MarketplaceConversation>
            MarketplaceConversations
        { get; set; }

        public DbSet<MarketplaceMessage>
            MarketplaceMessages
        { get; set; }


        // =========================================
        // INQUIRY
        // =========================================

        public DbSet<Inquiry>
            Inquiries
        { get; set; }


        // =========================================
        // AUDIT LOG
        // =========================================

        public DbSet<AuditLog>
            AuditLogs
        { get; set; }


        


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =========================================
            // UNIQUE VALUES
            // =========================================

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();


            modelBuilder.Entity<Skill>()
                .HasIndex(x => x.SkillName)
                .IsUnique();


            modelBuilder.Entity<ServiceCategory>()
                .HasIndex(x => x.CategoryName)
                .IsUnique();


            modelBuilder.Entity<MarketplaceCategory>()
                .HasIndex(x => x.CategoryName)
                .IsUnique();


            // =========================================
            // CAREER MASTER DATA
            // =========================================

            modelBuilder.Entity<CareerCategory>()
                .HasIndex(x => x.CategoryName)
                .IsUnique();


            modelBuilder.Entity<CareerRole>()
                .HasIndex(x => new
                {
                    x.CareerCategoryId,
                    x.RoleName
                })
                .IsUnique();


            modelBuilder.Entity<CareerRoleSkill>()
                .HasIndex(x => new
                {
                    x.CareerRoleId,
                    x.SkillId
                })
                .IsUnique();


            // =========================================
            // GENERAL USER PROFILE
            // =========================================

            modelBuilder.Entity<UserPublicProfile>()
                .HasIndex(x => x.UserId)
                .IsUnique();


            modelBuilder.Entity<UserPublicProfile>()
                .HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<UserPublicProfile>(
                    x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<UserPublicProfileSkill>()
                .HasOne(x => x.UserPublicProfile)
                .WithMany(x => x.Skills)
                .HasForeignKey(
                    x => x.UserPublicProfileId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<UserPublicProfileSkill>()
                .HasIndex(x => new
                {
                    x.UserPublicProfileId,
                    x.SkillName
                })
                .IsUnique();


            modelBuilder.Entity<UserPublicProfileEducation>()
                .HasOne(x => x.UserPublicProfile)
                .WithMany(x => x.Educations)
                .HasForeignKey(
                    x => x.UserPublicProfileId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================
            // USER -> ROLE PROFILES
            // =========================================

            modelBuilder.Entity<User>()
                .HasOne(x => x.JobSeekerProfile)
                .WithOne(x => x.User)
                .HasForeignKey<JobSeekerProfile>(
                    x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<User>()
                .HasOne(x => x.ServiceProviderProfile)
                .WithOne(x => x.User)
                .HasForeignKey<ServiceProviderProfile>(
                    x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<User>()
                .HasOne(x => x.MarketplaceSellerProfile)
                .WithOne(x => x.User)
                .HasForeignKey<MarketplaceSellerProfile>(
                    x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<User>()
                .HasOne(x => x.CompanyProfile)
                .WithOne(x => x.User)
                .HasForeignKey<CompanyProfile>(
                    x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================
            // CAREER CATEGORY -> CAREER ROLES
            // =========================================

            modelBuilder.Entity<CareerCategory>()
                .HasMany(x => x.CareerRoles)
                .WithOne(x => x.CareerCategory)
                .HasForeignKey(
                    x => x.CareerCategoryId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================
            // CAREER ROLE -> ROLE SKILLS
            // =========================================

            modelBuilder.Entity<CareerRole>()
                .HasMany(x => x.CareerRoleSkills)
                .WithOne(x => x.CareerRole)
                .HasForeignKey(
                    x => x.CareerRoleId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<CareerRoleSkill>()
                .HasOne(x => x.Skill)
                .WithMany()
                .HasForeignKey(
                    x => x.SkillId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================
            // JOB -> CAREER ROLE
            // =========================================

            modelBuilder.Entity<Job>()
                .HasOne(x => x.CareerRole)
                .WithMany()
                .HasForeignKey(
                    x => x.CareerRoleId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Job>()
                .HasIndex(x => x.CareerRoleId);


            // =========================================
            // JOB SEEKER SKILLS
            // =========================================

            modelBuilder.Entity<JobSeekerSkill>()
                .HasIndex(x => new
                {
                    x.JobSeekerProfileId,
                    x.SkillId
                })
                .IsUnique();


            // =========================================
            // JOB SKILLS
            // =========================================

            modelBuilder.Entity<JobSkill>()
                .HasIndex(x => new
                {
                    x.JobId,
                    x.SkillId
                })
                .IsUnique();


            // =========================================
            // JOB APPLICATION
            // =========================================

            modelBuilder.Entity<JobApplication>()
                .HasIndex(x => new
                {
                    x.JobId,
                    x.JobSeekerProfileId
                })
                .IsUnique();


            // =========================================
            // CV MATCH RESULT
            // =========================================

            modelBuilder.Entity<CVMatchResult>()
                .HasOne(x => x.JobApplication)
                .WithOne()
                .HasForeignKey<CVMatchResult>(
                    x => x.JobApplicationId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<CVMatchResult>()
                .HasIndex(x => x.JobApplicationId)
                .IsUnique();


            // =========================================
            // SCREENING / PAID CV ANALYSIS
            // =========================================

            modelBuilder.Entity<JobScreeningQuestion>()
                .HasOne(x => x.Job)
                .WithMany()
                .HasForeignKey(x => x.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JobScreeningQuestion>()
                .HasIndex(x => new { x.JobId, x.DisplayOrder });

            modelBuilder.Entity<JobScreeningAnswer>()
                .HasOne(x => x.JobApplication)
                .WithMany()
                .HasForeignKey(x => x.JobApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JobScreeningAnswer>()
                .HasOne(x => x.JobScreeningQuestion)
                .WithMany(x => x.Answers)
                .HasForeignKey(x => x.JobScreeningQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JobScreeningAnswer>()
                .HasIndex(x => new { x.JobApplicationId, x.JobScreeningQuestionId })
                .IsUnique();

            modelBuilder.Entity<CVAnalysisPurchase>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CVAnalysisPurchase>()
                .HasOne(x => x.Job)
                .WithMany()
                .HasForeignKey(x => x.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CVAnalysisPurchase>()
                .HasOne(x => x.CVDocument)
                .WithMany()
                .HasForeignKey(x => x.CVDocumentId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================
            // MARKETPLACE MESSAGING
            // =========================================

            modelBuilder.Entity<MarketplaceConversation>()
                .HasOne(x => x.MarketplaceListing)
                .WithMany()
                .HasForeignKey(x => x.MarketplaceListingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MarketplaceConversation>()
                .HasOne(x => x.BuyerUser)
                .WithMany()
                .HasForeignKey(x => x.BuyerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MarketplaceConversation>()
                .HasOne(x => x.SellerUser)
                .WithMany()
                .HasForeignKey(x => x.SellerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MarketplaceConversation>()
                .HasIndex(x => new { x.MarketplaceListingId, x.BuyerUserId })
                .IsUnique();

            modelBuilder.Entity<MarketplaceMessage>()
                .HasOne(x => x.MarketplaceConversation)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.MarketplaceConversationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MarketplaceMessage>()
                .HasOne(x => x.SenderUser)
                .WithMany()
                .HasForeignKey(x => x.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================
            // USER NOTIFICATIONS
            // =========================================

            modelBuilder.Entity<UserNotification>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserNotification>()
                .HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });

            // =========================================
            // SERVICE REVIEW
            // =========================================

            modelBuilder.Entity<ServiceReview>()
                .HasIndex(x => x.ServiceRequestId)
                .IsUnique();


            modelBuilder.Entity<ServiceReview>()
                .HasOne(x => x.ServiceRequest)
                .WithOne(x => x.Review)
                .HasForeignKey<ServiceReview>(
                    x => x.ServiceRequestId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<ServiceReview>()
                .HasOne(x => x.ServiceProviderProfile)
                .WithMany()
                .HasForeignKey(
                    x => x.ServiceProviderProfileId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<ServiceReview>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(
                    x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================
            // INQUIRIES
            // =========================================

            modelBuilder.Entity<Inquiry>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Inquiry>()
                .HasIndex(x => x.Status);


            modelBuilder.Entity<Inquiry>()
                .HasIndex(x => x.InquiryType);


            modelBuilder.Entity<Inquiry>()
                .HasIndex(x => x.CreatedAt);


            modelBuilder.Entity<Inquiry>()
                .HasIndex(x => x.Email);


            // =========================================
            // AUDIT LOGS
            // =========================================

            modelBuilder.Entity<AuditLog>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<AuditLog>()
                .HasIndex(x => x.UserId);


            modelBuilder.Entity<AuditLog>()
                .HasIndex(x => x.Action);


            modelBuilder.Entity<AuditLog>()
                .HasIndex(x => x.EntityType);


            modelBuilder.Entity<AuditLog>()
                .HasIndex(x => x.CreatedAt);


            // =========================================
            // DECIMAL - JOBS
            // =========================================

            modelBuilder.Entity<Job>()
                .Property(x => x.SalaryMin)
                .HasPrecision(18, 2);


            modelBuilder.Entity<Job>()
                .Property(x => x.SalaryMax)
                .HasPrecision(18, 2);


            modelBuilder.Entity<Job>()
                .Property(x => x.MinimumExperienceYears)
                .HasPrecision(5, 2);


            // =========================================
            // DECIMAL - JOB SEEKER
            // =========================================

            modelBuilder.Entity<JobSeekerProfile>()
                .Property(x => x.ExpectedSalary)
                .HasPrecision(18, 2);


            modelBuilder.Entity<JobSeekerSkill>()
                .Property(x => x.YearsOfExperience)
                .HasPrecision(5, 2);


            modelBuilder.Entity<JobSkill>()
                .Property(x => x.RequiredYearsOfExperience)
                .HasPrecision(5, 2);


            // =========================================
            // DECIMAL - CAREER INTELLIGENCE
            // =========================================

            modelBuilder.Entity<CareerRole>()
                .Property(x =>
                    x.BaselineExperienceYears)
                .HasPrecision(5, 2);


            modelBuilder.Entity<CareerRoleSkill>()
                .Property(x =>
                    x.RecommendedYearsOfExperience)
                .HasPrecision(5, 2);


            // =========================================
            // DECIMAL - SERVICES
            // =========================================

            modelBuilder.Entity<ServiceProviderProfile>()
                .Property(x => x.StartingPrice)
                .HasPrecision(18, 2);


            modelBuilder.Entity<ServiceOffering>()
                .Property(x => x.StartingPrice)
                .HasPrecision(18, 2);


            modelBuilder.Entity<ServiceRequest>()
                .Property(x => x.Budget)
                .HasPrecision(18, 2);


            modelBuilder.Entity<ServiceOffer>()
                .Property(x => x.OfferedPrice)
                .HasPrecision(18, 2);


            // =========================================
            // DECIMAL - MARKETPLACE
            // =========================================

            modelBuilder.Entity<MarketplaceListing>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);


            modelBuilder.Entity<CVAnalysisPurchase>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CVAnalysisPurchase>()
                .Property(x => x.MatchPercentage)
                .HasPrecision(5, 2);

            // =========================================
            // DECIMAL - CV MATCH
            // =========================================

            modelBuilder.Entity<CVMatchResult>()
                .Property(x => x.SkillsScore)
                .HasPrecision(5, 2);


            modelBuilder.Entity<CVMatchResult>()
                .Property(x => x.ExperienceScore)
                .HasPrecision(5, 2);


            modelBuilder.Entity<CVMatchResult>()
                .Property(x => x.EducationScore)
                .HasPrecision(5, 2);


            modelBuilder.Entity<CVMatchResult>()
                .Property(x => x.PreferredSkillsScore)
                .HasPrecision(5, 2);


            modelBuilder.Entity<CVMatchResult>()
                .Property(x => x.RelevanceScore)
                .HasPrecision(5, 2);


            modelBuilder.Entity<CVMatchResult>()
                .Property(x => x.LocationScore)
                .HasPrecision(5, 2);


            modelBuilder.Entity<CVMatchResult>()
                .Property(x => x.OverallScore)
                .HasPrecision(5, 2);


            // =========================================
            // PREVENT MULTIPLE CASCADE PATHS
            // =========================================

            foreach (var foreignKey in
                     modelBuilder.Model
                         .GetEntityTypes()
                         .SelectMany(
                             entity =>
                                 entity.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior =
                    DeleteBehavior.Restrict;
            }
        }
    }
}
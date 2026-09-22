using Microsoft.EntityFrameworkCore;

namespace WorkHub.API.Data
{
    /// <summary>
    /// Idempotent bootstrap for the final-project modules that were added after the
    /// original EF migration set. It is safe to run repeatedly and does not store
    /// payment card data. For production, convert this schema into formal EF migrations.
    /// </summary>
    public static class WorkHubFeatureSchemaInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var sql = @"
IF COL_LENGTH('dbo.UserPublicProfiles', 'NationalIdNumber') IS NULL
BEGIN
    ALTER TABLE dbo.UserPublicProfiles ADD NationalIdNumber NVARCHAR(50) NULL;
END;

IF OBJECT_ID(N'dbo.UserNotifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserNotifications
    (
        UserNotificationId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL,
        Title NVARCHAR(160) NOT NULL,
        Message NVARCHAR(1500) NOT NULL,
        NotificationType NVARCHAR(60) NOT NULL CONSTRAINT DF_UserNotifications_Type DEFAULT('System'),
        RelatedInquiryId INT NULL,
        IsRead BIT NOT NULL CONSTRAINT DF_UserNotifications_IsRead DEFAULT(0),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_UserNotifications_CreatedAt DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT FK_UserNotifications_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
    );
    CREATE INDEX IX_UserNotifications_UserReadCreated ON dbo.UserNotifications(UserId, IsRead, CreatedAt DESC);
END;

IF OBJECT_ID(N'dbo.JobScreeningQuestions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.JobScreeningQuestions
    (
        JobScreeningQuestionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        JobId INT NOT NULL,
        QuestionText NVARCHAR(500) NOT NULL,
        QuestionType NVARCHAR(30) NOT NULL,
        OptionsJson NVARCHAR(1500) NULL,
        IsRequired BIT NOT NULL,
        DisplayOrder INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_JobScreeningQuestions_Jobs FOREIGN KEY (JobId) REFERENCES dbo.Jobs(JobId)
    );
    CREATE INDEX IX_JobScreeningQuestions_Job_Order ON dbo.JobScreeningQuestions(JobId, DisplayOrder);
END;

IF OBJECT_ID(N'dbo.JobScreeningAnswers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.JobScreeningAnswers
    (
        JobScreeningAnswerId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        JobApplicationId INT NOT NULL,
        JobScreeningQuestionId INT NOT NULL,
        AnswerText NVARCHAR(2000) NOT NULL,
        SubmittedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_JobScreeningAnswers_Applications FOREIGN KEY (JobApplicationId) REFERENCES dbo.JobApplications(JobApplicationId),
        CONSTRAINT FK_JobScreeningAnswers_Questions FOREIGN KEY (JobScreeningQuestionId) REFERENCES dbo.JobScreeningQuestions(JobScreeningQuestionId)
    );
    CREATE UNIQUE INDEX UX_JobScreeningAnswers_App_Question ON dbo.JobScreeningAnswers(JobApplicationId, JobScreeningQuestionId);
END;

IF OBJECT_ID(N'dbo.MarketplaceConversations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MarketplaceConversations
    (
        MarketplaceConversationId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MarketplaceListingId INT NOT NULL,
        BuyerUserId INT NOT NULL,
        SellerUserId INT NOT NULL,
        Status NVARCHAR(20) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_MarketplaceConversations_Listings FOREIGN KEY (MarketplaceListingId) REFERENCES dbo.MarketplaceListings(MarketplaceListingId),
        CONSTRAINT FK_MarketplaceConversations_Buyer FOREIGN KEY (BuyerUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_MarketplaceConversations_Seller FOREIGN KEY (SellerUserId) REFERENCES dbo.Users(UserId)
    );
    CREATE UNIQUE INDEX UX_MarketplaceConversations_Listing_Buyer ON dbo.MarketplaceConversations(MarketplaceListingId, BuyerUserId);
    CREATE INDEX IX_MarketplaceConversations_Buyer ON dbo.MarketplaceConversations(BuyerUserId, UpdatedAt DESC);
    CREATE INDEX IX_MarketplaceConversations_Seller ON dbo.MarketplaceConversations(SellerUserId, UpdatedAt DESC);
END;

IF OBJECT_ID(N'dbo.MarketplaceMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MarketplaceMessages
    (
        MarketplaceMessageId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MarketplaceConversationId INT NOT NULL,
        SenderUserId INT NOT NULL,
        MessageText NVARCHAR(2000) NOT NULL,
        SentAt DATETIME2 NOT NULL,
        ReadAt DATETIME2 NULL,
        CONSTRAINT FK_MarketplaceMessages_Conversation FOREIGN KEY (MarketplaceConversationId) REFERENCES dbo.MarketplaceConversations(MarketplaceConversationId),
        CONSTRAINT FK_MarketplaceMessages_Sender FOREIGN KEY (SenderUserId) REFERENCES dbo.Users(UserId)
    );
    CREATE INDEX IX_MarketplaceMessages_Conversation_Sent ON dbo.MarketplaceMessages(MarketplaceConversationId, SentAt);
END;

IF OBJECT_ID(N'dbo.CVAnalysisPurchases', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CVAnalysisPurchases
    (
        CVAnalysisPurchaseId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL,
        JobId INT NOT NULL,
        CVDocumentId INT NOT NULL,
        PlanCode NVARCHAR(30) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Currency NVARCHAR(20) NOT NULL,
        PaymentStatus NVARCHAR(30) NOT NULL,
        PaymentReference NVARCHAR(80) NULL,
        PaymentMethod NVARCHAR(30) NULL,
        CardLast4 NVARCHAR(4) NULL,
        MatchPercentage DECIMAL(5,2) NOT NULL,
        MissingSkills NVARCHAR(4000) NULL,
        Recommendations NVARCHAR(4000) NULL,
        PurchasedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_CVAnalysisPurchases_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_CVAnalysisPurchases_Jobs FOREIGN KEY (JobId) REFERENCES dbo.Jobs(JobId),
        CONSTRAINT FK_CVAnalysisPurchases_CV FOREIGN KEY (CVDocumentId) REFERENCES dbo.CVDocuments(CVDocumentId)
    );
    CREATE INDEX IX_CVAnalysisPurchases_User_Date ON dbo.CVAnalysisPurchases(UserId, PurchasedAt DESC);
END;
";

            await db.Database.ExecuteSqlRawAsync(sql);
            await SeedCategoriesAsync(db);
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext db)
        {
            if (!await db.ServiceCategories.AnyAsync())
            {
                db.ServiceCategories.AddRange(
                    new WorkHub.API.Models.ServiceCategory { CategoryName = "Plumbing", Description = "Plumbing repairs, installations and maintenance", IsActive = true },
                    new WorkHub.API.Models.ServiceCategory { CategoryName = "Electrical", Description = "Residential and commercial electrical services", IsActive = true },
                    new WorkHub.API.Models.ServiceCategory { CategoryName = "Masonry", Description = "Brick, block, concrete and repair work", IsActive = true },
                    new WorkHub.API.Models.ServiceCategory { CategoryName = "Carpentry", Description = "Furniture, fittings and timber work", IsActive = true },
                    new WorkHub.API.Models.ServiceCategory { CategoryName = "Cleaning", Description = "Home, office and end-of-lease cleaning", IsActive = true },
                    new WorkHub.API.Models.ServiceCategory { CategoryName = "IT Support", Description = "Computer, network and software support", IsActive = true },
                    new WorkHub.API.Models.ServiceCategory { CategoryName = "Tutoring", Description = "Academic and professional tutoring", IsActive = true },
                    new WorkHub.API.Models.ServiceCategory { CategoryName = "Gardening", Description = "Garden maintenance and landscaping", IsActive = true });
            }

            if (!await db.MarketplaceCategories.AnyAsync())
            {
                db.MarketplaceCategories.AddRange(
                    new WorkHub.API.Models.MarketplaceCategory { CategoryName = "Vehicles", Description = "Cars, bikes and vehicle-related listings", IsActive = true },
                    new WorkHub.API.Models.MarketplaceCategory { CategoryName = "Property", Description = "Land, houses and property listings", IsActive = true },
                    new WorkHub.API.Models.MarketplaceCategory { CategoryName = "Rentals", Description = "Items and property available to rent", IsActive = true },
                    new WorkHub.API.Models.MarketplaceCategory { CategoryName = "Electronics", Description = "Phones, computers and electronics", IsActive = true },
                    new WorkHub.API.Models.MarketplaceCategory { CategoryName = "Home & Garden", Description = "Furniture, appliances and garden items", IsActive = true },
                    new WorkHub.API.Models.MarketplaceCategory { CategoryName = "Fashion", Description = "Clothing and accessories", IsActive = true },
                    new WorkHub.API.Models.MarketplaceCategory { CategoryName = "Other", Description = "Other marketplace items", IsActive = true });
            }

            if (db.ChangeTracker.HasChanges())
                await db.SaveChangesAsync();
        }
    }
}

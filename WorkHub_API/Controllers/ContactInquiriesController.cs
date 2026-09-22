using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using WorkHub.API.DTOs;

namespace WorkHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactInquiriesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ContactInquiriesController(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // =====================================================
        // CREATE CONTACT INQUIRY
        // POST: api/ContactInquiries
        // =====================================================

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create(
            [FromBody] ContactInquiryCreateDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var connectionString =
                _configuration.GetConnectionString(
                    "DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Database connection is not configured."
                    });
            }

            try
            {
                await using var connection =
                    new SqlConnection(connectionString);

                await connection.OpenAsync();

                int? validUserId = null;

                // =================================================
                // VALIDATE USER ID
                // =================================================

                if (request.UserId.HasValue &&
                    request.UserId.Value > 0)
                {
                    const string userCheckSql = @"
                        SELECT COUNT(1)
                        FROM dbo.Users
                        WHERE UserId = @UserId;
                    ";

                    await using var userCheckCommand =
                        new SqlCommand(
                            userCheckSql,
                            connection);

                    userCheckCommand.Parameters.AddWithValue(
                        "@UserId",
                        request.UserId.Value);

                    var count =
                        Convert.ToInt32(
                            await userCheckCommand
                                .ExecuteScalarAsync());

                    if (count > 0)
                    {
                        validUserId =
                            request.UserId.Value;
                    }
                }

                // =================================================
                // FALLBACK:
                // MATCH REGISTERED USER BY EMAIL
                // =================================================

                if (!validUserId.HasValue)
                {
                    const string emailUserSql = @"
                        SELECT TOP 1 UserId
                        FROM dbo.Users
                        WHERE LOWER(Email) =
                              LOWER(@Email);
                    ";

                    await using var emailCommand =
                        new SqlCommand(
                            emailUserSql,
                            connection);

                    emailCommand.Parameters.AddWithValue(
                        "@Email",
                        request.Email.Trim());

                    var emailResult =
                        await emailCommand
                            .ExecuteScalarAsync();

                    if (emailResult != null &&
                        emailResult != DBNull.Value)
                    {
                        validUserId =
                            Convert.ToInt32(emailResult);
                    }
                }

                // =================================================
                // INSERT INTO EXISTING Inquiries TABLE
                // =================================================

                const string sql = @"
                    INSERT INTO dbo.Inquiries
                    (
                        UserId,
                        Name,
                        Email,
                        Phone,
                        InquiryType,
                        Subject,
                        Message,
                        Status,
                        CreatedAt
                    )
                    OUTPUT INSERTED.InquiryId
                    VALUES
                    (
                        @UserId,
                        @Name,
                        @Email,
                        NULL,
                        @InquiryType,
                        @Subject,
                        @Message,
                        'New',
                        SYSUTCDATETIME()
                    );
                ";

                await using var command =
                    new SqlCommand(
                        sql,
                        connection);

                command.Parameters.AddWithValue(
                    "@UserId",
                    validUserId.HasValue
                        ? validUserId.Value
                        : DBNull.Value);

                command.Parameters.AddWithValue(
                    "@Name",
                    request.FullName.Trim());

                command.Parameters.AddWithValue(
                    "@Email",
                    request.Email.Trim());

                command.Parameters.AddWithValue(
                    "@InquiryType",
                    request.Topic.Trim());

                command.Parameters.AddWithValue(
                    "@Subject",
                    request.Topic.Trim());

                command.Parameters.AddWithValue(
                    "@Message",
                    request.Message.Trim());

                var result =
                    await command.ExecuteScalarAsync();

                var inquiryId =
                    Convert.ToInt32(result);

                return Ok(
                    new
                    {
                        message =
                            "Your inquiry has been submitted successfully.",

                        inquiryId =
                            inquiryId
                    });
            }
            catch (SqlException)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Unable to save the inquiry."
                    });
            }
        }

        // =====================================================
        // ADMIN GET ALL
        // GET: api/ContactInquiries/admin
        // =====================================================

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var connectionString =
                _configuration.GetConnectionString(
                    "DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Database connection is not configured."
                    });
            }

            var inquiries =
                new List<object>();

            try
            {
                await using var connection =
                    new SqlConnection(connectionString);

                await connection.OpenAsync();

                const string sql = @"
                    SELECT
                        InquiryId,
                        UserId,
                        Name,
                        Email,
                        Phone,
                        InquiryType,
                        Subject,
                        Message,
                        Status,
                        AdminNote,
                        CreatedAt,
                        UpdatedAt,
                        ReplyMessage,
                        RepliedAt,
                        RepliedBy
                    FROM dbo.Inquiries
                    ORDER BY CreatedAt DESC;
                ";

                await using var command =
                    new SqlCommand(
                        sql,
                        connection);

                await using var reader =
                    await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    inquiries.Add(
                        new
                        {
                            inquiryId =
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "InquiryId")),

                            userId =
                                reader.IsDBNull(
                                    reader.GetOrdinal(
                                        "UserId"))
                                    ? (int?)null
                                    : reader.GetInt32(
                                        reader.GetOrdinal(
                                            "UserId")),

                            name =
                                reader.GetString(
                                    reader.GetOrdinal(
                                        "Name")),

                            email =
                                reader.GetString(
                                    reader.GetOrdinal(
                                        "Email")),

                            phone =
                                reader.IsDBNull(
                                    reader.GetOrdinal(
                                        "Phone"))
                                    ? null
                                    : reader.GetString(
                                        reader.GetOrdinal(
                                            "Phone")),

                            inquiryType =
                                reader.GetString(
                                    reader.GetOrdinal(
                                        "InquiryType")),

                            subject =
                                reader.GetString(
                                    reader.GetOrdinal(
                                        "Subject")),

                            message =
                                reader.GetString(
                                    reader.GetOrdinal(
                                        "Message")),

                            status =
                                reader.GetString(
                                    reader.GetOrdinal(
                                        "Status")),

                            adminNote =
                                reader.IsDBNull(
                                    reader.GetOrdinal(
                                        "AdminNote"))
                                    ? null
                                    : reader.GetString(
                                        reader.GetOrdinal(
                                            "AdminNote")),

                            createdAt =
                                reader.GetDateTime(
                                    reader.GetOrdinal(
                                        "CreatedAt")),

                            updatedAt =
                                reader.IsDBNull(
                                    reader.GetOrdinal(
                                        "UpdatedAt"))
                                    ? (DateTime?)null
                                    : reader.GetDateTime(
                                        reader.GetOrdinal(
                                            "UpdatedAt")),

                            replyMessage =
                                reader.IsDBNull(
                                    reader.GetOrdinal(
                                        "ReplyMessage"))
                                    ? null
                                    : reader.GetString(
                                        reader.GetOrdinal(
                                            "ReplyMessage")),

                            repliedAt =
                                reader.IsDBNull(
                                    reader.GetOrdinal(
                                        "RepliedAt"))
                                    ? (DateTime?)null
                                    : reader.GetDateTime(
                                        reader.GetOrdinal(
                                            "RepliedAt")),

                            repliedBy =
                                reader.IsDBNull(
                                    reader.GetOrdinal(
                                        "RepliedBy"))
                                    ? null
                                    : reader.GetString(
                                        reader.GetOrdinal(
                                            "RepliedBy"))
                        });
                }

                return Ok(inquiries);
            }
            catch (SqlException)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Unable to load inquiries."
                    });
            }
        }

        // =====================================================
        // ADMIN GET DETAILS
        // GET: api/ContactInquiries/admin/2
        // =====================================================

        [HttpGet("admin/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetForAdmin(
            int id)
        {
            var connectionString =
                _configuration.GetConnectionString(
                    "DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Database connection is not configured."
                    });
            }

            try
            {
                await using var connection =
                    new SqlConnection(connectionString);

                await connection.OpenAsync();

                const string sql = @"
                    SELECT
                        InquiryId,
                        UserId,
                        Name,
                        Email,
                        Phone,
                        InquiryType,
                        Subject,
                        Message,
                        Status,
                        AdminNote,
                        CreatedAt,
                        UpdatedAt,
                        ReplyMessage,
                        RepliedAt,
                        RepliedBy
                    FROM dbo.Inquiries
                    WHERE InquiryId = @Id;
                ";

                await using var command =
                    new SqlCommand(
                        sql,
                        connection);

                command.Parameters.AddWithValue(
                    "@Id",
                    id);

                await using var reader =
                    await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(
                        new
                        {
                            message =
                                "Inquiry not found."
                        });
                }

                var result =
                    new
                    {
                        inquiryId =
                            reader.GetInt32(
                                reader.GetOrdinal(
                                    "InquiryId")),

                        userId =
                            reader.IsDBNull(
                                reader.GetOrdinal(
                                    "UserId"))
                                ? (int?)null
                                : reader.GetInt32(
                                    reader.GetOrdinal(
                                        "UserId")),

                        name =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Name")),

                        email =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Email")),

                        phone =
                            reader.IsDBNull(
                                reader.GetOrdinal(
                                    "Phone"))
                                ? null
                                : reader.GetString(
                                    reader.GetOrdinal(
                                        "Phone")),

                        inquiryType =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "InquiryType")),

                        subject =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Subject")),

                        message =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Message")),

                        status =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Status")),

                        adminNote =
                            reader.IsDBNull(
                                reader.GetOrdinal(
                                    "AdminNote"))
                                ? null
                                : reader.GetString(
                                    reader.GetOrdinal(
                                        "AdminNote")),

                        createdAt =
                            reader.GetDateTime(
                                reader.GetOrdinal(
                                    "CreatedAt")),

                        updatedAt =
                            reader.IsDBNull(
                                reader.GetOrdinal(
                                    "UpdatedAt"))
                                ? (DateTime?)null
                                : reader.GetDateTime(
                                    reader.GetOrdinal(
                                        "UpdatedAt")),

                        replyMessage =
                            reader.IsDBNull(
                                reader.GetOrdinal(
                                    "ReplyMessage"))
                                ? null
                                : reader.GetString(
                                    reader.GetOrdinal(
                                        "ReplyMessage")),

                        repliedAt =
                            reader.IsDBNull(
                                reader.GetOrdinal(
                                    "RepliedAt"))
                                ? (DateTime?)null
                                : reader.GetDateTime(
                                    reader.GetOrdinal(
                                        "RepliedAt")),

                        repliedBy =
                            reader.IsDBNull(
                                reader.GetOrdinal(
                                    "RepliedBy"))
                                ? null
                                : reader.GetString(
                                    reader.GetOrdinal(
                                        "RepliedBy"))
                    };

                return Ok(result);
            }
            catch (SqlException)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Unable to load inquiry."
                    });
            }
        }

        // =====================================================
        // ADMIN REPLY
        //
        // PUT:
        // api/ContactInquiries/admin/2/reply
        // =====================================================

        [HttpPut("admin/{id:int}/reply")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reply(
            int id,
            [FromBody] AdminInquiryReplyRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(
                    request.ReplyMessage))
            {
                return BadRequest(
                    new
                    {
                        message =
                            "Please enter a reply."
                    });
            }

            var replyMessage =
                request.ReplyMessage.Trim();

            if (replyMessage.Length > 4000)
            {
                return BadRequest(
                    new
                    {
                        message =
                            "Reply cannot exceed 4000 characters."
                    });
            }

            var connectionString =
                _configuration.GetConnectionString(
                    "DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Database connection is not configured."
                    });
            }

            var adminName =
                User.FindFirstValue(
                    ClaimTypes.Name)
                ??
                User.FindFirstValue("name")
                ??
                "WorkHub Admin";

            try
            {
                await using var connection =
                    new SqlConnection(connectionString);

                await connection.OpenAsync();

                await using var transaction =
                    await connection.BeginTransactionAsync();

                try
                {
                    // =========================================
                    // GET INQUIRY
                    // =========================================

                    const string inquirySql = @"
                        SELECT
                            UserId,
                            Email,
                            InquiryType,
                            Subject
                        FROM dbo.Inquiries
                        WHERE InquiryId = @Id;
                    ";

                    int? userId = null;

                    string inquiryEmail =
                        string.Empty;

                    string inquiryTopic =
                        "Contact Inquiry";

                    await using (
                        var inquiryCommand =
                            new SqlCommand(
                                inquirySql,
                                connection,
                                (SqlTransaction)transaction))
                    {
                        inquiryCommand.Parameters.AddWithValue(
                            "@Id",
                            id);

                        await using var reader =
                            await inquiryCommand
                                .ExecuteReaderAsync();

                        if (!await reader.ReadAsync())
                        {
                            await transaction.RollbackAsync();

                            return NotFound(
                                new
                                {
                                    message =
                                        "Inquiry not found."
                                });
                        }

                        var userIdOrdinal =
                            reader.GetOrdinal("UserId");

                        if (!reader.IsDBNull(userIdOrdinal))
                        {
                            userId =
                                reader.GetInt32(
                                    userIdOrdinal);
                        }

                        inquiryEmail =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Email"));

                        inquiryTopic =
                            reader.GetString(
                                reader.GetOrdinal(
                                    "Subject"));
                    }

                    // =========================================
                    // MATCH USER BY EMAIL IF NEEDED
                    // =========================================

                    if (!userId.HasValue &&
                        !string.IsNullOrWhiteSpace(
                            inquiryEmail))
                    {
                        const string findUserSql = @"
                            SELECT TOP 1 UserId
                            FROM dbo.Users
                            WHERE LOWER(Email) =
                                  LOWER(@Email);
                        ";

                        await using var findUserCommand =
                            new SqlCommand(
                                findUserSql,
                                connection,
                                (SqlTransaction)transaction);

                        findUserCommand.Parameters.AddWithValue(
                            "@Email",
                            inquiryEmail);

                        var userResult =
                            await findUserCommand
                                .ExecuteScalarAsync();

                        if (userResult != null &&
                            userResult != DBNull.Value)
                        {
                            userId =
                                Convert.ToInt32(userResult);

                            const string linkUserSql = @"
                                UPDATE dbo.Inquiries
                                SET UserId = @UserId
                                WHERE InquiryId = @Id;
                            ";

                            await using var linkCommand =
                                new SqlCommand(
                                    linkUserSql,
                                    connection,
                                    (SqlTransaction)transaction);

                            linkCommand.Parameters.AddWithValue(
                                "@UserId",
                                userId.Value);

                            linkCommand.Parameters.AddWithValue(
                                "@Id",
                                id);

                            await linkCommand
                                .ExecuteNonQueryAsync();
                        }
                    }

                    // =========================================
                    // SAVE ADMIN REPLY
                    // =========================================

                    const string updateSql = @"
                        UPDATE dbo.Inquiries
                        SET
                            ReplyMessage = @ReplyMessage,
                            RepliedAt = SYSUTCDATETIME(),
                            RepliedBy = @RepliedBy,
                            Status = 'Replied',
                            UpdatedAt = SYSUTCDATETIME()
                        WHERE InquiryId = @Id;
                    ";

                    await using var updateCommand =
                        new SqlCommand(
                            updateSql,
                            connection,
                            (SqlTransaction)transaction);

                    updateCommand.Parameters.AddWithValue(
                        "@ReplyMessage",
                        replyMessage);

                    updateCommand.Parameters.AddWithValue(
                        "@RepliedBy",
                        adminName);

                    updateCommand.Parameters.AddWithValue(
                        "@Id",
                        id);

                    var rowsAffected =
                        await updateCommand
                            .ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        await transaction.RollbackAsync();

                        return NotFound(
                            new
                            {
                                message =
                                    "Inquiry not found."
                            });
                    }

                    // =========================================
                    // CREATE USER NOTIFICATION
                    // =========================================

                    if (userId.HasValue)
                    {
                        const string notificationSql = @"
                            INSERT INTO dbo.UserNotifications
                            (
                                UserId,
                                Title,
                                Message,
                                NotificationType,
                                RelatedInquiryId,
                                IsRead,
                                CreatedAt
                            )
                            VALUES
                            (
                                @UserId,
                                @Title,
                                @Message,
                                'InquiryReply',
                                @RelatedInquiryId,
                                0,
                                SYSUTCDATETIME()
                            );
                        ";

                        await using var notificationCommand =
                            new SqlCommand(
                                notificationSql,
                                connection,
                                (SqlTransaction)transaction);

                        notificationCommand.Parameters.AddWithValue(
                            "@UserId",
                            userId.Value);

                        notificationCommand.Parameters.AddWithValue(
                            "@Title",
                            "Admin replied to your inquiry");

                        notificationCommand.Parameters.AddWithValue(
                            "@Message",
                            $"WorkHub Admin replied to your inquiry: \"{inquiryTopic}\"");

                        notificationCommand.Parameters.AddWithValue(
                            "@RelatedInquiryId",
                            id);

                        await notificationCommand
                            .ExecuteNonQueryAsync();
                    }

                    // =========================================
                    // COMMIT
                    // =========================================

                    await transaction.CommitAsync();

                    return Ok(
                        new
                        {
                            message =
                                userId.HasValue
                                    ? "Reply saved and user notification created successfully."
                                    : "Reply saved successfully. No registered user account was linked to this inquiry.",

                            notificationCreated =
                                userId.HasValue,

                            linkedUserId =
                                userId
                        });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (SqlException)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Unable to save the reply or notification."
                    });
            }
        }

        // =====================================================
        // REQUEST MODEL
        // =====================================================

        public class AdminInquiryReplyRequest
        {
            public string ReplyMessage { get; set; }
                = string.Empty;
        }
    }
}
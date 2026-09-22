using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace WorkHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "RegisteredUser")]
    public class UserNotificationsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UserNotificationsController(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }


        // =====================================================
        // GET MY NOTIFICATIONS
        // GET:
        // api/UserNotifications/me
        // =====================================================

        [HttpGet("me")]
        public async Task<IActionResult> GetMyNotifications()
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

                var userId =
                    await ResolveCurrentUserIdAsync(
                        connection);

                if (!userId.HasValue)
                {
                    return Unauthorized(
                        new
                        {
                            message =
                                "Unable to identify the current user."
                        });
                }


                // =================================================
                // SYNCHRONISE ADMIN REPLIES
                // =================================================

                await SyncInquiryRepliesAsync(
                    connection,
                    userId.Value);


                // =================================================
                // LOAD NOTIFICATIONS
                // =================================================

                const string sql = @"
                    SELECT
                        n.UserNotificationId,
                        n.UserId,

                        CASE
                            WHEN i.InquiryType IS NOT NULL
                                 AND LTRIM(RTRIM(i.InquiryType)) <> ''
                            THEN CONCAT(
                                'Admin replied: ',
                                i.InquiryType
                            )
                            ELSE n.Title
                        END AS Title,

                        CASE
                            WHEN i.ReplyMessage IS NOT NULL
                                 AND LTRIM(RTRIM(i.ReplyMessage)) <> ''
                            THEN i.ReplyMessage
                            ELSE n.Message
                        END AS Message,

                        n.NotificationType,
                        n.RelatedInquiryId,
                        n.IsRead,

                        CASE
                            WHEN i.RepliedAt IS NOT NULL
                            THEN i.RepliedAt
                            ELSE n.CreatedAt
                        END AS CreatedAt

                    FROM dbo.UserNotifications n

                    LEFT JOIN dbo.Inquiries i
                        ON i.InquiryId =
                           n.RelatedInquiryId

                    WHERE n.UserId = @UserId

                    ORDER BY
                        n.IsRead ASC,
                        CreatedAt DESC;
                ";


                await using var command =
                    new SqlCommand(
                        sql,
                        connection);

                command.Parameters.AddWithValue(
                    "@UserId",
                    userId.Value);


                var notifications =
                    new List<object>();


                await using var reader =
                    await command.ExecuteReaderAsync();


                while (await reader.ReadAsync())
                {
                    var relatedInquiryOrdinal =
                        reader.GetOrdinal(
                            "RelatedInquiryId");


                    notifications.Add(
                        new
                        {
                            userNotificationId =
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "UserNotificationId")),

                            userId =
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "UserId")),

                            title =
                                reader.IsDBNull(
                                    reader.GetOrdinal(
                                        "Title"))
                                    ? string.Empty
                                    : reader.GetString(
                                        reader.GetOrdinal(
                                            "Title")),

                            message =
                                reader.IsDBNull(
                                    reader.GetOrdinal(
                                        "Message"))
                                    ? string.Empty
                                    : reader.GetString(
                                        reader.GetOrdinal(
                                            "Message")),

                            notificationType =
                                reader.IsDBNull(
                                    reader.GetOrdinal(
                                        "NotificationType"))
                                    ? string.Empty
                                    : reader.GetString(
                                        reader.GetOrdinal(
                                            "NotificationType")),

                            relatedInquiryId =
                                reader.IsDBNull(
                                    relatedInquiryOrdinal)
                                    ? (int?)null
                                    : reader.GetInt32(
                                        relatedInquiryOrdinal),

                            isRead =
                                reader.GetBoolean(
                                    reader.GetOrdinal(
                                        "IsRead")),

                            createdAt =
                                reader.GetDateTime(
                                    reader.GetOrdinal(
                                        "CreatedAt"))
                        });
                }


                return Ok(notifications);
            }
            catch (SqlException ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Unable to load notifications.",

                        detail =
                            ex.Message
                    });
            }
        }


        // =====================================================
        // MARK ONE NOTIFICATION AS READ
        // PUT:
        // api/UserNotifications/5/read
        // =====================================================

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(
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
                    new SqlConnection(
                        connectionString);

                await connection.OpenAsync();


                var userId =
                    await ResolveCurrentUserIdAsync(
                        connection);


                if (!userId.HasValue)
                {
                    return Unauthorized();
                }


                const string sql = @"
                    UPDATE dbo.UserNotifications

                    SET IsRead = 1

                    WHERE
                        UserNotificationId = @Id
                        AND UserId = @UserId;
                ";


                await using var command =
                    new SqlCommand(
                        sql,
                        connection);


                command.Parameters.AddWithValue(
                    "@Id",
                    id);

                command.Parameters.AddWithValue(
                    "@UserId",
                    userId.Value);


                var affected =
                    await command.ExecuteNonQueryAsync();


                if (affected == 0)
                {
                    return NotFound(
                        new
                        {
                            message =
                                "Notification not found."
                        });
                }


                return Ok(
                    new
                    {
                        message =
                            "Notification marked as read."
                    });
            }
            catch (SqlException)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Unable to update notification."
                    });
            }
        }


        // =====================================================
        // MARK ALL NOTIFICATIONS AS READ
        // PUT:
        // api/UserNotifications/read-all
        // =====================================================

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
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
                    new SqlConnection(
                        connectionString);

                await connection.OpenAsync();


                var userId =
                    await ResolveCurrentUserIdAsync(
                        connection);


                if (!userId.HasValue)
                {
                    return Unauthorized();
                }


                const string sql = @"
                    UPDATE dbo.UserNotifications

                    SET IsRead = 1

                    WHERE
                        UserId = @UserId
                        AND IsRead = 0;
                ";


                await using var command =
                    new SqlCommand(
                        sql,
                        connection);


                command.Parameters.AddWithValue(
                    "@UserId",
                    userId.Value);


                await command.ExecuteNonQueryAsync();


                return Ok(
                    new
                    {
                        message =
                            "All notifications marked as read."
                    });
            }
            catch (SqlException)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Unable to update notifications."
                    });
            }
        }


        // =====================================================
        // SYNCHRONISE ADMIN REPLIES
        // =====================================================

        private static async Task SyncInquiryRepliesAsync(
            SqlConnection connection,
            int userId)
        {
            // =================================================
            // STEP 1
            //
            // Link old inquiries to the current user
            // using the user's email.
            // =================================================

            const string linkOldSql = @"
                UPDATE i

                SET i.UserId = @UserId

                FROM dbo.Inquiries i

                INNER JOIN dbo.Users u
                    ON u.UserId = @UserId

                WHERE
                    i.UserId IS NULL

                    AND

                    LOWER(
                        LTRIM(
                            RTRIM(i.Email)
                        )
                    )
                    =
                    LOWER(
                        LTRIM(
                            RTRIM(u.Email)
                        )
                    );
            ";


            await using (
                var linkCommand =
                    new SqlCommand(
                        linkOldSql,
                        connection))
            {
                linkCommand.Parameters.AddWithValue(
                    "@UserId",
                    userId);

                await linkCommand.ExecuteNonQueryAsync();
            }


            // =================================================
            // STEP 2
            //
            // Update existing inquiry-reply notifications.
            // =================================================

            const string updateSql = @"
                UPDATE n

                SET
                    IsRead =
                        CASE
                            WHEN ISNULL(n.Message, '')
                                 <>
                                 LEFT(
                                     ISNULL(i.ReplyMessage, ''),
                                     1000
                                 )
                            THEN 0
                            ELSE n.IsRead
                        END,

                    Title =
                        LEFT(
                            CONCAT(
                                'Admin replied: ',
                                i.InquiryType
                            ),
                            200
                        ),

                    Message =
                        LEFT(
                            i.ReplyMessage,
                            1000
                        ),

                    CreatedAt =
                        CASE
                            WHEN ISNULL(n.Message, '')
                                 <>
                                 LEFT(
                                     ISNULL(i.ReplyMessage, ''),
                                     1000
                                 )
                            THEN ISNULL(
                                i.RepliedAt,
                                SYSUTCDATETIME()
                            )
                            ELSE n.CreatedAt
                        END

                FROM dbo.UserNotifications n

                INNER JOIN dbo.Inquiries i
                    ON i.InquiryId =
                       n.RelatedInquiryId

                WHERE
                    n.UserId = @UserId

                    AND

                    n.NotificationType =
                        'InquiryReply'

                    AND

                    i.UserId = @UserId

                    AND

                    i.ReplyMessage IS NOT NULL

                    AND

                    LTRIM(
                        RTRIM(i.ReplyMessage)
                    ) <> '';
            ";


            await using (
                var updateCommand =
                    new SqlCommand(
                        updateSql,
                        connection))
            {
                updateCommand.Parameters.AddWithValue(
                    "@UserId",
                    userId);

                await updateCommand.ExecuteNonQueryAsync();
            }


            // =================================================
            // STEP 3
            //
            // Create notifications for admin replies
            // that do not already have notifications.
            // =================================================

            const string insertSql = @"
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

                SELECT
                    i.UserId,

                    LEFT(
                        CONCAT(
                            'Admin replied: ',
                            i.InquiryType
                        ),
                        200
                    ),

                    LEFT(
                        i.ReplyMessage,
                        1000
                    ),

                    'InquiryReply',

                    i.InquiryId,

                    0,

                    ISNULL(
                        i.RepliedAt,
                        SYSUTCDATETIME()
                    )

                FROM dbo.Inquiries i

                WHERE
                    i.UserId = @UserId

                    AND

                    i.ReplyMessage IS NOT NULL

                    AND

                    LTRIM(
                        RTRIM(i.ReplyMessage)
                    ) <> ''

                    AND

                    NOT EXISTS
                    (
                        SELECT 1

                        FROM dbo.UserNotifications n

                        WHERE
                            n.UserId = i.UserId

                            AND

                            n.RelatedInquiryId =
                                i.InquiryId

                            AND

                            n.NotificationType =
                                'InquiryReply'
                    );
            ";


            await using (
                var insertCommand =
                    new SqlCommand(
                        insertSql,
                        connection))
            {
                insertCommand.Parameters.AddWithValue(
                    "@UserId",
                    userId);

                await insertCommand.ExecuteNonQueryAsync();
            }
        }


        // =====================================================
        // FIND CURRENT USER
        // =====================================================

        private async Task<int?> ResolveCurrentUserIdAsync(
            SqlConnection connection)
        {
            var possibleClaims =
                new[]
                {
                    ClaimTypes.NameIdentifier,
                    "UserId",
                    "userId",
                    "userid",
                    "sub"
                };


            foreach (var claimName in possibleClaims)
            {
                var value =
                    User.FindFirstValue(
                        claimName);

                if (
                    int.TryParse(
                        value,
                        out var id)
                    &&
                    id > 0)
                {
                    return id;
                }
            }


            // =================================================
            // FALLBACK:
            // Resolve user using JWT email.
            // =================================================

            var email =
                User.FindFirstValue(
                    ClaimTypes.Email)
                ??
                User.FindFirstValue(
                    "email");


            if (string.IsNullOrWhiteSpace(
                    email))
            {
                return null;
            }


            const string sql = @"
                SELECT TOP 1 UserId

                FROM dbo.Users

                WHERE
                    LOWER(Email) =
                    LOWER(@Email);
            ";


            await using var command =
                new SqlCommand(
                    sql,
                    connection);


            command.Parameters.AddWithValue(
                "@Email",
                email.Trim());


            var result =
                await command.ExecuteScalarAsync();


            if (
                result == null
                ||
                result == DBNull.Value)
            {
                return null;
            }


            return Convert.ToInt32(result);
        }
    }
}
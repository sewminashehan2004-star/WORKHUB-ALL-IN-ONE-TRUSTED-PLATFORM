using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using WorkHub.API.Data;
using WorkHub.API.Models;

namespace WorkHub.API.Controllers
{
    [ApiController]
    [Route("api/MarketplaceMessages")]
    [Authorize(Roles = "RegisteredUser")]
    public class MarketplaceMessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MarketplaceMessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("listing/{listingId:int}/conversation")]
        public async Task<IActionResult> StartConversation(int listingId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var listing = await _context.MarketplaceListings
                .Where(x => x.MarketplaceListingId == listingId && x.Status == "Active")
                .Select(x => new
                {
                    x.MarketplaceListingId,
                    SellerUserId = x.MarketplaceSellerProfile.UserId,
                    x.Title
                })
                .FirstOrDefaultAsync();

            if (listing == null)
                return NotFound(new { message = "Marketplace listing not found." });

            if (listing.SellerUserId == userId.Value)
                return BadRequest(new { message = "You cannot start a buyer conversation with your own listing." });

            var conversation = await _context.MarketplaceConversations
                .FirstOrDefaultAsync(x =>
                    x.MarketplaceListingId == listingId &&
                    x.BuyerUserId == userId.Value);

            if (conversation == null)
            {
                conversation = new MarketplaceConversation
                {
                    MarketplaceListingId = listingId,
                    BuyerUserId = userId.Value,
                    SellerUserId = listing.SellerUserId,
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.MarketplaceConversations.Add(conversation);
                await _context.SaveChangesAsync();
            }
            else if (!string.Equals(conversation.Status, "Open", StringComparison.OrdinalIgnoreCase))
            {
                conversation.Status = "Open";
                conversation.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                conversation.MarketplaceConversationId,
                listing.MarketplaceListingId,
                listing.Title,
                conversation.Status
            });
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var items = await _context.MarketplaceConversations
                .Where(x => x.BuyerUserId == userId.Value || x.SellerUserId == userId.Value)
                .OrderByDescending(x => x.UpdatedAt)
                .Select(x => new
                {
                    x.MarketplaceConversationId,
                    x.Status,
                    x.UpdatedAt,
                    Listing = new
                    {
                        x.MarketplaceListingId,
                        x.MarketplaceListing.Title,
                        x.MarketplaceListing.Price,
                        x.MarketplaceListing.ListingType
                    },
                    OtherParty = new
                    {
                        UserId = x.BuyerUserId == userId.Value ? x.SellerUserId : x.BuyerUserId,
                        FullName = x.BuyerUserId == userId.Value ? x.SellerUser.FullName : x.BuyerUser.FullName
                    },
                    LastMessage = x.Messages
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => new { m.MessageText, m.SentAt, m.SenderUserId })
                        .FirstOrDefault(),
                    UnreadCount = x.Messages.Count(m => m.SenderUserId != userId.Value && m.ReadAt == null)
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("conversations/{conversationId:int}")]
        public async Task<IActionResult> GetConversation(int conversationId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var conversation = await _context.MarketplaceConversations
                .Where(x => x.MarketplaceConversationId == conversationId &&
                            (x.BuyerUserId == userId.Value || x.SellerUserId == userId.Value))
                .Select(x => new
                {
                    x.MarketplaceConversationId,
                    x.Status,
                    Listing = new
                    {
                        x.MarketplaceListingId,
                        x.MarketplaceListing.Title,
                        x.MarketplaceListing.Price,
                        x.MarketplaceListing.ListingType,
                        x.MarketplaceListing.Location
                    },
                    Buyer = new { x.BuyerUserId, x.BuyerUser.FullName },
                    Seller = new { x.SellerUserId, x.SellerUser.FullName }
                })
                .FirstOrDefaultAsync();

            if (conversation == null)
                return NotFound(new { message = "Conversation not found." });

            var unread = await _context.MarketplaceMessages
                .Where(x => x.MarketplaceConversationId == conversationId &&
                            x.SenderUserId != userId.Value &&
                            x.ReadAt == null)
                .ToListAsync();

            foreach (var message in unread)
                message.ReadAt = DateTime.UtcNow;

            if (unread.Count > 0)
                await _context.SaveChangesAsync();

            var messages = await _context.MarketplaceMessages
                .Where(x => x.MarketplaceConversationId == conversationId)
                .OrderBy(x => x.SentAt)
                .Select(x => new
                {
                    x.MarketplaceMessageId,
                    x.SenderUserId,
                    SenderName = x.SenderUser.FullName,
                    x.MessageText,
                    x.SentAt,
                    x.ReadAt,
                    IsMine = x.SenderUserId == userId.Value
                })
                .ToListAsync();

            return Ok(new { conversation, messages });
        }

        [HttpPost("conversations/{conversationId:int}/messages")]
        public async Task<IActionResult> SendMessage(int conversationId, SendMarketplaceMessageRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (request == null || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "Please enter a message." });

            var conversation = await _context.MarketplaceConversations
                .FirstOrDefaultAsync(x =>
                    x.MarketplaceConversationId == conversationId &&
                    (x.BuyerUserId == userId.Value || x.SellerUserId == userId.Value));

            if (conversation == null)
                return NotFound(new { message = "Conversation not found." });

            if (!string.Equals(conversation.Status, "Open", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "This conversation is closed." });

            var text = request.Message.Trim();
            if (text.Length > 2000)
                return BadRequest(new { message = "Message cannot exceed 2000 characters." });

            var message = new MarketplaceMessage
            {
                MarketplaceConversationId = conversationId,
                SenderUserId = userId.Value,
                MessageText = text,
                SentAt = DateTime.UtcNow
            };

            conversation.UpdatedAt = DateTime.UtcNow;
            _context.MarketplaceMessages.Add(message);

            var recipientUserId = conversation.BuyerUserId == userId.Value
                ? conversation.SellerUserId
                : conversation.BuyerUserId;

            _context.UserNotifications.Add(new UserNotification
            {
                UserId = recipientUserId,
                Title = "New marketplace message",
                Message = "You received a new message about a marketplace listing.",
                NotificationType = "MarketplaceMessage",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message.MarketplaceMessageId,
                message.MessageText,
                message.SentAt
            });
        }

        [HttpPost("conversations/{conversationId:int}/close")]
        public async Task<IActionResult> CloseConversation(int conversationId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var conversation = await _context.MarketplaceConversations
                .FirstOrDefaultAsync(x => x.MarketplaceConversationId == conversationId &&
                    (x.BuyerUserId == userId.Value || x.SellerUserId == userId.Value));

            if (conversation == null)
                return NotFound(new { message = "Conversation not found." });

            conversation.Status = "Closed";
            conversation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Conversation closed." });
        }

        private int? GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }

        public class SendMarketplaceMessageRequest
        {
            [Required, MaxLength(2000)]
            public string Message { get; set; } = string.Empty;
        }
    }
}

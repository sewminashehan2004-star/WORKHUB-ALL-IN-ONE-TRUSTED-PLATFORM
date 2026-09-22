using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Workhub_Web.Controllers
{
    public class UserNewsController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public UserNewsController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =
                httpClientFactory;
        }


        // =========================================
        // GET PUBLISHED NEWS FOR PROFILE
        //
        // GET:
        // /UserNews/List
        // =========================================

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var token =
                HttpContext.Session
                    .GetString("JwtToken");


            var role =
                HttpContext.Session
                    .GetString("Role");


            if (
                string.IsNullOrWhiteSpace(token)
                ||
                !string.Equals(
                    role,
                    "RegisteredUser",
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                return Unauthorized(
                    new
                    {
                        message =
                            "Please sign in to view news and updates."
                    });
            }


            try
            {
                var client =
                    _httpClientFactory
                        .CreateClient(
                            "WorkHubApi");


                var response =
                    await client.GetAsync(
                        "api/NewsUpdates/public");


                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        (int)response.StatusCode,
                        new
                        {
                            message =
                                "Unable to load news and updates."
                        });
                }


                var items =
                    await response.Content
                        .ReadFromJsonAsync<
                            List<UserNewsItem>>()
                    ??
                    new List<UserNewsItem>();


                return Ok(items);
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    503,
                    new
                    {
                        message =
                            "Unable to connect to WorkHub API."
                    });
            }
        }


        // =========================================
        // RESPONSE MODEL
        // =========================================

        private class UserNewsItem
        {
            public int NewsUpdateId
            {
                get;
                set;
            }


            public string Title
            {
                get;
                set;
            } = string.Empty;


            public string? Summary
            {
                get;
                set;
            }


            public string Content
            {
                get;
                set;
            } = string.Empty;


            public string Category
            {
                get;
                set;
            } = "General";


            public bool IsPublished
            {
                get;
                set;
            }


            public DateTime? PublishedAt
            {
                get;
                set;
            }


            public DateTime CreatedAt
            {
                get;
                set;
            }


            public DateTime? UpdatedAt
            {
                get;
                set;
            }


            public string? CreatedBy
            {
                get;
                set;
            }
        }
    }
}
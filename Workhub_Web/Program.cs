var builder = WebApplication.CreateBuilder(args);

// =====================================
// MVC
// =====================================

builder.Services.AddControllersWithViews();

// =====================================
// WORKHUB API HTTP CLIENT
// =====================================

builder.Services.AddHttpClient(
    "WorkHubApi",
    client =>
    {
        var apiBaseUrl =
            builder.Configuration[
                "ApiSettings:BaseUrl"
            ];

        if (string.IsNullOrWhiteSpace(
                apiBaseUrl))
        {
            throw new InvalidOperationException(
                "WorkHub API BaseUrl is missing."
            );
        }

        client.BaseAddress =
            new Uri(apiBaseUrl);
    });

// =====================================
// SESSION
// =====================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout =
        TimeSpan.FromHours(2);

    options.Cookie.HttpOnly =
        true;

    options.Cookie.IsEssential =
        true;
});

// =====================================
// HTTP CONTEXT
// =====================================

builder.Services.AddHttpContextAccessor();

// =====================================
// BUILD APP
// =====================================

var app = builder.Build();

// =====================================
// ERROR HANDLING
// =====================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error");

    app.UseHsts();
}

// =====================================
// HTTP PIPELINE
// =====================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// =====================================
// ROUTES
// =====================================

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
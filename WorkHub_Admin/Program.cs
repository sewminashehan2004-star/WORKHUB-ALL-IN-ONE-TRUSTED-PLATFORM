var builder = WebApplication.CreateBuilder(args);


// =============================================
// MVC
// =============================================

builder.Services.AddControllersWithViews();


// =============================================
// WORKHUB API CLIENT
// =============================================

builder.Services.AddHttpClient(
    "WorkHubApi",
    client =>
    {
        client.BaseAddress =
            new Uri(
                "https://localhost:7015/");
    });


// =============================================
// SESSION
//
// Admin JWT token and admin information
// will be stored here after login.
// =============================================

builder.Services.AddDistributedMemoryCache();


builder.Services.AddSession(options =>
{
    options.IdleTimeout =
        TimeSpan.FromHours(2);

    options.Cookie.HttpOnly =
        true;

    options.Cookie.IsEssential =
        true;

    options.Cookie.Name =
        ".WorkHub.Admin.Session";
});


builder.Services.AddHttpContextAccessor();


var app = builder.Build();


// =============================================
// ERROR HANDLING
// =============================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error");

    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseStaticFiles();


app.UseRouting();


// =============================================
// SESSION
// =============================================

app.UseSession();


app.UseAuthorization();


// =============================================
// ROUTES
// =============================================

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=AdminAuth}/{action=Login}/{id?}");


app.Run();
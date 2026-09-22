using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Threading.RateLimiting;
using WorkHub.API.Data;
using WorkHub.API.Interfaces;
using WorkHub.API.Middleware;
using WorkHub.API.Models;
using WorkHub.API.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================
// CONTROLLERS
// =====================================

builder.Services.AddControllers();

// =====================================
// OPENAPI / SCALAR
// =====================================

builder.Services.AddOpenApi();

// =====================================
// DATABASE
// =====================================

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection")
        )
);

// =====================================
// PASSWORD HASHING
// =====================================

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>
>();

// =====================================
// TOKEN SERVICE
// =====================================

builder.Services.AddScoped<
    ITokenService,
    TokenService
>();

// =====================================
// CV MATCHING SERVICE
// =====================================

builder.Services.AddScoped<
    ICVMatchingService,
    CVMatchingService
>();

// =====================================
// CV TEXT EXTRACTION SERVICE
// =====================================

builder.Services.AddScoped<
    ICVTextExtractionService,
    CVTextExtractionService
>();

// =====================================
// CAREER BENCHMARK SERVICE
// =====================================

builder.Services.AddScoped<
    ICareerBenchmarkService,
    CareerBenchmarkService
>();

// =====================================
// CV INTELLIGENCE SERVICE
// =====================================

builder.Services.AddScoped<
    ICVIntelligenceService,
    CVIntelligenceService
>();

// =====================================
// AUDIT LOG SERVICE
// =====================================

builder.Services.AddScoped<
    IAuditLogService,
    AuditLogService
>();

// =====================================
// HTTP CONTEXT
// =====================================

builder.Services.AddHttpContextAccessor();

// =====================================
// CORS
// =====================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "WorkHubFrontend",
        policy =>
        {
            policy
                .SetIsOriginAllowed(origin =>
                {
                    if (!Uri.TryCreate(
                            origin,
                            UriKind.Absolute,
                            out var uri))
                    {
                        return false;
                    }

                    return uri.Host.Equals(
                               "localhost",
                               StringComparison.OrdinalIgnoreCase)
                           ||
                           uri.Host.Equals(
                               "127.0.0.1",
                               StringComparison.OrdinalIgnoreCase);
                })
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// =====================================
// RATE LIMITING
// =====================================

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter =
        PartitionedRateLimiter
            .Create<HttpContext, string>(
                httpContext =>
                {
                    var clientKey =
                        httpContext.Connection
                            .RemoteIpAddress?
                            .ToString()
                        ?? "unknown";

                    return RateLimitPartition
                        .GetFixedWindowLimiter(
                            clientKey,
                            _ =>
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 120,

                                    Window =
                                        TimeSpan.FromMinutes(1),

                                    QueueLimit = 0,

                                    AutoReplenishment = true
                                });
                });

    options.OnRejected =
        async (
            context,
            cancellationToken) =>
        {
            context.HttpContext.Response
                .ContentType =
                "application/json";

            await context.HttpContext.Response
                .WriteAsJsonAsync(
                    new
                    {
                        message =
                            "Too many requests. Please try again shortly."
                    },
                    cancellationToken);
        };
});

// =====================================
// JWT SETTINGS
// =====================================

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT Key is missing."
    );

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT Issuer is missing."
    );

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT Audience is missing."
    );

// =====================================
// JWT AUTHENTICATION
// =====================================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,

                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtKey)
                    ),

                ClockSkew = TimeSpan.Zero
            };

        options.Events =
            new JwtBearerEvents
            {
                OnChallenge =
                    async context =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode =
                            StatusCodes
                                .Status401Unauthorized;

                        context.Response.ContentType =
                            "application/json";

                        await context.Response
                            .WriteAsJsonAsync(
                                new
                                {
                                    message =
                                        "Authentication is required."
                                });
                    },

                OnForbidden =
                    async context =>
                    {
                        context.Response.StatusCode =
                            StatusCodes
                                .Status403Forbidden;

                        context.Response.ContentType =
                            "application/json";

                        await context.Response
                            .WriteAsJsonAsync(
                                new
                                {
                                    message =
                                        "You do not have permission to access this resource."
                                });
                    }
            };
    });

// =====================================
// AUTHORIZATION
// =====================================

builder.Services.AddAuthorization();

// =====================================
// BUILD APPLICATION
// =====================================

var app =
    builder.Build();

// =====================================
// CAREER MASTER DATA
// =====================================

await CareerDataSeeder.SeedAsync(
    app.Services);

// =====================================
// CAREER BASELINE DATA
// =====================================

await CareerBaselineSeeder.SeedAsync(
    app.Services);

// =====================================
// FINAL PROJECT FEATURE TABLES
// =====================================

await WorkHubFeatureSchemaInitializer.InitializeAsync(
    app.Services);

// =====================================
// DEVELOPMENT
// =====================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference();
}
else
{
    app.UseHsts();
}

// =====================================
// SECURITY HEADERS
// =====================================

app.Use(async (context, next) =>
{
    context.Response.Headers
        .TryAdd(
            "X-Content-Type-Options",
            "nosniff");

    context.Response.Headers
        .TryAdd(
            "X-Frame-Options",
            "DENY");

    context.Response.Headers
        .TryAdd(
            "Referrer-Policy",
            "strict-origin-when-cross-origin");

    context.Response.Headers
        .TryAdd(
            "Permissions-Policy",
            "camera=(), microphone=(), geolocation=()");

    await next();
});

// =====================================
// HTTP PIPELINE
// =====================================

app.UseHttpsRedirection();

app.UseCors(
    "WorkHubFrontend");

app.UseRateLimiter();

app.UseAuthentication();

app.UseMiddleware<
    AccountStatusMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
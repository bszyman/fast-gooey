using FastGooey.Composers;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.Configuration;
using FastGooey.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using Fido2NetLib;
using Fido2NetLib.Serialization;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
#if DEBUG
    .AddRazorRuntimeCompilation()
#endif
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.AllowOutOfOrderMetadataProperties = true;
        x.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        x.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, FidoModelSerializerContext.Default);
    });

builder.Services.AddCoreServices();
builder.Services.AddCloudflareTurnstileServices();
builder.Services.AddAppleMapKitServices();
builder.Services.AddAppleWeatherKitServices();
builder.Services.AddSingleton<IClock>(SystemClock.Instance);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            o => o.UseNodaTime())
    .UseSnakeCaseNamingConvention());
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.Configure<SmtpConfigurationModel>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<Fido2Configuration>(builder.Configuration.GetSection("Fido2"));
builder.Services.AddSingleton<Fido2>(sp =>
{
    var config = sp.GetRequiredService<IOptions<Fido2Configuration>>().Value;
    return new Fido2(config);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Set to true if you want email confirmation
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
    })
    .AddOpenIdConnect("Apple", "Sign in with Apple", options =>
    {
        options.Authority = "https://appleid.apple.com";
        options.ClientId = builder.Configuration["Authentication:Apple:ClientId"]!;
        options.CallbackPath = "/signin-apple";
        options.ResponseType = "code id_token";
        options.ResponseMode = "form_post";
        options.DisableTelemetry = true;
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("name");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = "https://appleid.apple.com",
            ValidAudience = builder.Configuration["Authentication:Apple:ClientId"]!,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true
        };

        options.Events = new OpenIdConnectEvents
        {
            OnAuthorizationCodeReceived = context =>
            {
                var jwtService = context.HttpContext.RequestServices
                    .GetRequiredService<IAppleSignInJwtService>();
                context.TokenEndpointRequest!.ClientSecret = jwtService.GenerateClientSecret();

                return Task.CompletedTask;
            }
        };
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/auth/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    initializer.Initialize();
    
    if (app.Environment.IsProduction())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async(context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    
    await next();
});

app.UseCsp(csp =>
{
    csp.ByDefaultAllow.FromSelf();
    
    csp.AllowScripts
        .FromSelf()
        .AllowUnsafeInline()
        .From("https://unpkg.com")
        .From("https://cdn.apple-mapkit.com")
        .From("https://challenges.cloudflare.com");
    
    csp.AllowStyles
        .FromSelf()
        .AllowUnsafeInline()
        .From("https://fonts.googleapis.com");
    
    csp.AllowFonts
        .FromSelf()
        .From("https://fonts.gstatic.com");
    
    csp.AllowImages
        .FromSelf()
        .From("https:");
    
    csp.AllowConnections
        .ToSelf()
        .To("https://challenges.cloudflare.com")
        .To("https://*.apple-mapkit.com")
        .To("https://*.apple.com");
    
    csp.AllowFrames
        .FromSelf()
        .From("https://challenges.cloudflare.com");
    
    csp.AllowWorkers
        .FromSelf()
        .From("blob:");
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

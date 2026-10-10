using AzureSnapshotManager.Api.Authentication;
using AzureSnapshotManager.Api.Development;
using AzureSnapshotManager.Core.Interfaces;
using AzureSnapshotManager.Infrastructure.Advisory;
using AzureSnapshotManager.Infrastructure.Discovery;
using AzureSnapshotManager.Infrastructure.Export;
using AzureSnapshotManager.Infrastructure.Pricing;
using AzureSnapshotManager.Infrastructure.Security;
using AzureSnapshotManager.Infrastructure.Storage;
using Microsoft.Identity.Web;

// "--offline" is how the packaged desktop .exe runs (see run-offline.bat /
// README "Offline desktop mode"): no Azure deployment, no server to manage —
// just this one process on the user's own PC, reusing whatever Azure
// identity is already available there (their own `az login`), exactly like
// local `dotnet run` does. This check happens BEFORE WebApplication.CreateBuilder
// so it can also pick the Kestrel port before the host is built.
var offlineMode = args.Contains("--offline");

var builder = WebApplication.CreateBuilder(args);

if (offlineMode)
{
    // Port 0 = "give me any free port" — avoids clashing with anything else
    // already running on the user's machine (including another copy of this
    // exe). The actual assigned port is read back after startup, below.
    builder.WebHost.UseUrls("http://127.0.0.1:0");
}

// "Local mode" = dev auth handler + in-memory storage instead of Entra ID +
// Table Storage. It is on for `dotnet run` (Development), for the packaged
// offline .exe (--offline), and can also be switched on for a public demo
// host with DemoMode=true — in which case a DEMO_PASSWORD is REQUIRED,
// because the dev auth handler accepts anyone.
var isDevelopment = builder.Environment.IsDevelopment();
var demoMode = builder.Configuration.GetValue<bool>("DemoMode");
var localMode = isDevelopment || demoMode || offlineMode;

var demoUser = builder.Configuration["DEMO_USER"] ?? "demo";
var demoPassword = builder.Configuration["DEMO_PASSWORD"];
if (demoMode && !isDevelopment && string.IsNullOrWhiteSpace(demoPassword))
{
    throw new InvalidOperationException(
        "DemoMode=true outside Development requires DEMO_PASSWORD to be set, " +
        "otherwise the open dev auth handler would be publicly reachable.");
}

// ---------------------------------------------------------------------
// WBS 1.6 Security & Access Control
// "Authenticate & Access Dashboard" use case: Microsoft Entra ID (Azure AD)
// bearer-token auth in every real environment.
//
// In Development ONLY, swap in DevelopmentAuthHandler so [Authorize]
// endpoints can be exercised locally (e.g. via Swagger) without a JWT —
// every request is auto-authenticated as a fake local user. The
// [Authorize] attributes on controllers are never removed; only which
// scheme satisfies them changes.
// ---------------------------------------------------------------------
if (localMode)
{
    builder.Services
        .AddAuthentication(DevelopmentAuthHandler.SchemeName)
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DevelopmentAuthHandler>(
            DevelopmentAuthHandler.SchemeName, _ => { });
}
else
{
    builder.Services
        .AddAuthentication(Microsoft.Identity.Web.Constants.Bearer)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
}

builder.Services.AddAuthorization();

// WBS 1.6.1 User-Assigned Managed Identity (shared credential provider).
builder.Services.AddSingleton<ManagedIdentityAuthProvider>();

// Lets the dashboard auto-detect the subscription instead of requiring
// manual entry — used by the offline desktop exe's auto-run flow.
builder.Services.AddSingleton<AzureSnapshotManager.Infrastructure.Account.AzureAccountService>();

// WBS 1.2 Architecture & Data Storage.
// In Development, use an in-memory repository so you can test discovery
// against a real Azure subscription without provisioning Table Storage
// first. Every other environment uses the real TableStorageRepository.
if (localMode)
{
    builder.Services.AddSingleton<ISnapshotRepository, InMemorySnapshotRepository>();
}
else
{
    builder.Services.AddSingleton<ISnapshotRepository, TableStorageRepository>();
}

// WBS 1.3 Inventory & Discovery Engine
builder.Services.AddSingleton<IDiscoveryEngine, ResourceGraphDiscoveryEngine>();

// WBS 1.3.2 Pricing (typed HttpClient + in-memory cache)
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IPricingService, RetailPricesApiService>();

// WBS 1.4 Advisory Analytics Engine
builder.Services.AddSingleton<IAdvisoryEngine, AdvisoryEngine>();

// WBS 1.5.2 Export
builder.Services.AddSingleton<IExportService, CsvExportService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Azure Snapshot Manager API",
        Version = "v1",
        Description = "Backend for the Azure Snapshot Manager Marketplace offer (WBS 1.1–1.6)."
    });
});

// CORS for the dashboard SPA (WBS 1.5.1). Restrict AllowedOrigins in
// appsettings/App Service config before going to production.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Dashboard", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                      ?? new[] { "http://localhost:5173" };
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Must be the FIRST middleware so static files, Swagger and the API are all
// behind the password prompt.
if (demoMode && !isDevelopment)
{
    app.UseMiddleware<DemoBasicAuthMiddleware>(demoUser, demoPassword!);
}

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Hosting platforms terminate TLS before the request reaches the app, and
// the offline exe only ever serves http://127.0.0.1, so redirecting in
// either case would only produce warnings/loops.
if (!demoMode && !offlineMode)
{
    app.UseHttpsRedirection();
}
app.UseCors("Dashboard");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Lightweight liveness endpoint for App Service health checks / Partner
// Center Marketplace validation.
app.MapGet("/healthz", () => Results.Ok(new { status = "healthy" })).AllowAnonymous();

if (offlineMode)
{
    // Wait for Kestrel to actually bind before reading back which port it
    // picked, then open the user's default browser straight to the dashboard.
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var addressFeature = app.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
            .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();
        var url = addressFeature?.Addresses.FirstOrDefault() ?? "http://127.0.0.1:5080";

        Console.WriteLine();
        Console.WriteLine("=========================================================");
        Console.WriteLine(" Azure Snapshot Manager — offline mode");
        Console.WriteLine($" Dashboard: {url}");
        Console.WriteLine(" Uses whichever Azure account you last ran 'az login' with.");
        Console.WriteLine(" Close this window to stop the audit tool.");
        Console.WriteLine("=========================================================");
        Console.WriteLine();

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            Console.WriteLine($"Could not auto-open a browser — open {url} manually.");
        }
    });
}

app.Run();

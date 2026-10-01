using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using StravaHeatmap.Api.Data;
using StravaHeatmap.Api.Options;
using StravaHeatmap.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Register the database connection.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Data protection keys. In Docker this folder must live on a persistent volume,
// otherwise sessions become invalid every time the container restarts.
var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(keysPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

// Bind the "Strava" section of appsettings.json to StravaOptions.
builder.Services.Configure<StravaOptions>(
    builder.Configuration.GetSection(StravaOptions.SectionName));

// HTTP client for Strava's OAuth endpoints.
// BaseAddress lets the service use short paths like "/oauth/token".
builder.Services.AddHttpClient<StravaAuthService>(client =>
{
    client.BaseAddress = new Uri("https://www.strava.com");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Separate client for Strava's data endpoints (longer timeout).
builder.Services.AddHttpClient<StravaApiService>(client =>
{
    client.BaseAddress = new Uri("https://www.strava.com");
    client.Timeout = TimeSpan.FromSeconds(60);
});

// Connection and token management.
builder.Services.AddScoped<StravaConnectionService>();
builder.Services.AddScoped<StravaSyncService>();

// Session: keeps the OAuth state value on the server for a short while.
// The data stays on the server; the browser only gets a cookie identifier.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;   // not readable from JavaScript
    options.Cookie.IsEssential = true;
});

// Register controllers.
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// In production the built React app is served from the wwwroot folder.
// During development that folder is empty and Vite serves the frontend instead.
app.UseDefaultFiles();
app.UseStaticFiles();

// Enable session support. Must run before the endpoints.
app.UseSession();

// Route incoming requests to controllers.
app.MapControllers();

// Anything that is not an API route falls through to the React app (SPA).
app.MapFallbackToFile("index.html");

app.Run();

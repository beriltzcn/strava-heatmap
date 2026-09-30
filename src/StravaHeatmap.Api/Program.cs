using Microsoft.EntityFrameworkCore;
using StravaHeatmap.Api.Data;
using StravaHeatmap.Api.Options;
using StravaHeatmap.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Veritabanı bağlantısını tanıt.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// appsettings.json'daki Strava bölümünü StravaOptions sınıfına bağla.
builder.Services.Configure<StravaOptions>(
    builder.Configuration.GetSection(StravaOptions.SectionName));

// Strava ile konuşacak HTTP istemcisi.
// BaseAddress sayesinde servis içinde "/oauth/token" gibi kısa yollar yazabiliyoruz.
builder.Services.AddHttpClient<StravaAuthService>(client =>
{
    client.BaseAddress = new Uri("https://www.strava.com");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Strava'nın veri uçları için istemci (token uçlarından ayrı).
builder.Services.AddHttpClient<StravaApiService>(client =>
{
    client.BaseAddress = new Uri("https://www.strava.com");
    client.Timeout = TimeSpan.FromSeconds(60);
});

// Bağlantı ve token yönetimi.
builder.Services.AddScoped<StravaConnectionService>();

// Oturum: OAuth state değerini kısa süreliğine sunucu tarafında tutmak için.
// Veri sunucuda kalır, tarayıcıya sadece bir çerez kimliği gider.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;   // JavaScript okuyamasın
    options.Cookie.IsEssential = true;
});

// Controller'ları tanı: "Controllers klasörüne bak, oradaki sınıfları kullan."
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Oturum desteğini devreye al. Controller'lardan önce gelmek zorunda.
app.UseSession();

// Gelen isteği, adresine göre doğru controller'a yönlendir.
app.MapControllers();

app.Run();

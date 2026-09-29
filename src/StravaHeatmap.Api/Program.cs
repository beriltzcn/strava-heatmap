using Microsoft.EntityFrameworkCore;
using StravaHeatmap.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Veritabanı bağlantısını tanıt.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controller'ları tanı: "Controllers klasörüne bak, oradaki sınıfları kullan."
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Gelen isteği, adresine göre doğru controller'a yönlendir.
app.MapControllers();

app.Run();
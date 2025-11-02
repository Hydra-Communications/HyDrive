using HyDrive.Api;
using HyDrive.Application;
using HyDrive.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var appSettings = builder.Configuration
      .GetSection("AppSettings")
      .Get<AppSettings>()
    ?? throw new InvalidOperationException("AppSettings section is missing or invalid.");

builder.Services.AddSingleton(appSettings);
builder.Services.AddInfrastructureServices();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var env = serviceProvider.GetRequiredService<IHostEnvironment>();
    var settings = serviceProvider.GetRequiredService<AppSettings>();

    if (env.IsDevelopment())
    {
        var dbPath = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dbPath);

        var fullPath = Path.Combine(dbPath, "hydrive.db");
        options.UseSqlite($"Data Source={fullPath}");

        Console.WriteLine($"Using SQLite database at: {fullPath}");
    }
    else
    {
        options.UseNpgsql(settings.DefaultConnection);
    }
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Run();
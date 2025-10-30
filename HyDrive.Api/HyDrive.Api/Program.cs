using HyDrive.Api;
using HyDrive.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var appSettings = new AppSettings();

builder.Services.AddSingleton(appSettings);

// Configure DbContext conditionally
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Add services to the container.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        options.UseSqlite(appSettings.SqliteConnection);    
    }
    else
    {
        options.UseNpgsql(appSettings.DefaultConnection);
    }
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Run();

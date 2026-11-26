using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using StockFlow.Infrastructure;
using StockFlow.Worker;
using StockFlow.Worker.HostedServices;

var builder = WebApplication.CreateBuilder(args);

// ---------- Đăng ký service ----------
// Worker chỉ cần DB, không cần JWT
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddHostedService<HeartbeatService>();

var app = builder.Build();

// ---------- Health check (App Service cần container trả lời HTTP) ----------
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapGet("/", () => "StockFlow Worker is running.");

app.Run();

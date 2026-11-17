using Microsoft.EntityFrameworkCore;
using StockFlow.Infrastructure;
using StockFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'Default' is missing.");
}


// Add services to the container.

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/api/db-check", async (AppDbContext db) => {
    var productCount = await db.Products.CountAsync();
    return Results.Ok(new { message = "Database connected", productCount });
});

app.Run();

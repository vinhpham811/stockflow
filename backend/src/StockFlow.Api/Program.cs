using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StockFlow.Api.MiddleWare;
using StockFlow.Application;
using StockFlow.Infrastructure;
using StockFlow.Infrastructure.Identity;
using StockFlow.Infrastructure.Persistence;

const string FrontendCorsPolicy = "Frontend";
var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'Default' is missing.");
}


// ---------- Đăng ký service ----------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "StockFlow API", Version = "v1" });
    // Thêm nút "Authorize" trên Swagger để dán access token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the access token (without the 'Bearer ' prefix)."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference{ Type= ReferenceType.SecurityScheme, Id="Bearer"}
            },
            Array.Empty<string>()
        }
    });
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // Bắt buộc để trình duyệt gửi cookie refresh token
    });
});
// Bắt đầu khai báo Global Exception Handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
//Kết thúc Global Exception Handling

var app = builder.Build();
// ---------- Seed role + tài khoản admin ----------
await IdentitySeeder.SeedAsync(app.Services, app.Configuration);

// ---------- Pipeline (thứ tự quan trọng) ----------
//app.UseMiddleware<ExceptionHandlingMiddleware>(); //Chỉ dành cho tự viết Middleware loại thông thường để xử lý Exception Global
// Trong pipeline: THAY dòng app.UseMiddleware<ExceptionHandlingMiddleware>(); bằng:
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

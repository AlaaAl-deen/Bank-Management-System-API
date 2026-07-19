using BankManagementSystem.Configurations;
using BankManagementSystem.Database;
using BankManagementSystem.Modules.Accounts.Services;
using BankManagementSystem.Modules.Authentication.Services;
using BankManagementSystem.Modules.Transactions.Services;
using BankManagementSystem.Modules.Users.Services;
using BankManagementSystem.Security;
using BankManagementSystem.Security.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Services
// ==========================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bank Management System API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Token like: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ==========================================
// CORS
// ==========================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ==========================================
// JWT
// ==========================================

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<IJwtService, JwtService>();

// ==========================================
// Dependency Injection
// ==========================================

builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<TransactionService>();

// ==========================================
// Authentication
// ==========================================

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer =
                builder.Configuration["Jwt:Issuer"],

            ValidAudience =
                builder.Configuration["Jwt:Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Key"]!))
        };
});

builder.Services.AddAuthorization();

// ==========================================
// Build
// ==========================================

var app = builder.Build();

// ==========================================
// Seed Admin (Optional)
// ==========================================

// var seeder = new DatabaseSeeder();
// seeder.SeedAdmin();

// ==========================================
// Middleware
// ==========================================

app.UseSwagger();

app.UseSwaggerUI();

app.UseAuthentication();

app.UseCors("AllowAll");

app.UseAuthorization();

// ==========================================
// Home Page
// ==========================================

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        Application = "Bank Management System API",
        Version = "1.0",
        Status = "Running",
        Swagger = "/swagger"
    });
});

// ==========================================
// Controllers
// ==========================================

app.MapControllers();

// ==========================================
// Run
// ==========================================

app.Run();
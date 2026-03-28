using EMS.API.Middleware;
using EMS.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Cors configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Controllers
builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi();

// Infrastructure (DB + JWT + All Services + repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Global Exception Handler — must be before all other middlewares
app.UseMiddleware<ExceptionMiddleware>();

// CORS
app.UseCors("AllowFrontend");

// OpenAPI + Scalar — both development and production
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "EMS API";
    options.Theme = ScalarTheme.Purple;
});

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
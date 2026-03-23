using EMS.API.Middleware;
using EMS.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Infrastructure (DB + JWT + All Services + Repos)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Global Exception Handler — sabse pehle
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors();

// OpenAPI + Scalar — dono environments mein
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "EMS API";
    options.Theme = ScalarTheme.Purple;
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
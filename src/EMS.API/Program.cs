using EMS.API.Middleware;
using EMS.Infrastructure;
using EMS.Infrastructure.Persistence;
using EMS.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:8080")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Controllers
builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi();

// Infrastructure (DB + JWT + All Services + Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// ── Database Migration + Seeding ──────────────────────────────────
// Har startup pe run hota hai — idempotent hai
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Pending migrations auto-apply karo
    await db.Database.MigrateAsync();

    // SuperAdmin seed karo agar exist nahi karta
    await SuperAdminSeeder.SeedAsync(db, builder.Configuration);
}

// ── Middleware Pipeline ───────────────────────────────────────────

// Global Exception Handler — sabse pehle
app.UseMiddleware<ExceptionMiddleware>();

// CORS
app.UseCors("AllowFrontend");

// OpenAPI + Scalar
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "EMS API";
    options.Theme = ScalarTheme.Purple;
});

// Authentication & Authorization — ORDER MATTERS
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
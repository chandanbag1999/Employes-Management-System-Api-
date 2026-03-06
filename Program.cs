var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Only enable HTTPS redirect if running behind a proxy that doesn't handle it
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection(); // Only in production
    app.UseHsts();             // Tells browsers to always use HTTPS
}


app.MapControllers();

app.Run();

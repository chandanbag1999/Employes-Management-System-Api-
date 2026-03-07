using EmployesManagementSystemApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container 
builder.Services.AddControllers();

// Register DbContext with PostgreSQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
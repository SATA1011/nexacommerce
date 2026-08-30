using Dapper;
using NexaCommerce.Data.Factories;
using NexaCommerce.Data.Migrations;
using NexaCommerce.Domain.Interfaces;
using NexaCommerce.Repository.Identity;

var builder = WebApplication.CreateBuilder(args);

// Configure Dapper to map MySQL snake_case columns (e.g., first_name) to C# PascalCase properties (FirstName)
DefaultTypeMap.MatchNamesWithUnderscores = true;

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Register Connection Factory
builder.Services.AddScoped<IDbConnectionFactory>(_ => new DbConnectionFactory(connectionString));

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Run Database Migrations (Table.sql & AllStoredProcedure.sql via DbUp)
var databaseDirectory = Path.Combine(app.Environment.ContentRootPath, "database");
try
{
    DatabaseMigrator.Migrate(connectionString, databaseDirectory);
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Database migration on startup was skipped or encountered an error. Ensure local MySQL service is running.");
}

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "NexaCommerce API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

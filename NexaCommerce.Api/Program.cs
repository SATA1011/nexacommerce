using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NexaCommerce.Api.Filters;
using NexaCommerce.Data.Factories;
using NexaCommerce.Domain.Interfaces;
using NexaCommerce.Repository.Identity;
using NexaCommerce.Security.Cryptography;
using NexaCommerce.Security.Options;
using NexaCommerce.Security.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Configure Dapper to map MySQL snake_case columns (e.g., first_name) to C# PascalCase properties (FirstName)
DefaultTypeMap.MatchNamesWithUnderscores = true;

// Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure JwtOptions
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() 
    ?? new JwtOptions();

// Register Security & Infrastructure Services
builder.Services.AddScoped<IDbConnectionFactory>(_ => new DbConnectionFactory(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

// Configure JWT Authentication with resilient token parsing
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authHeader))
            {
                if (authHeader.StartsWith("Bearer Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authHeader["Bearer Bearer ".Length..].Trim();
                }
                else if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authHeader["Bearer ".Length..].Trim();
                }
                else
                {
                    context.Token = authHeader.Trim();
                }
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin", "SuperAdmin"));
    options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer", "Admin", "SuperAdmin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User", "Customer", "Admin", "SuperAdmin"));
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger UI with Bearer Auth Scheme & Operation Filter
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "NexaCommerce API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT access token below (without 'Bearer ' prefix):"
    });

    options.OperationFilter<AuthorizeCheckOperationFilter>();
});

var app = builder.Build();


// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "NexaCommerce API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

using CodedByKay.BondBridge.API.DBContext;
using CodedByKay.BondBridge.API.Exstensions;
using CodedByKay.BondBridge.API.Middleware;
using CodedByKay.BondBridge.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

// Initialize a new WebApplication builder
var builder = WebApplication.CreateBuilder(args);

// Configure and register ApplicationSettings from appsettings.json into the DI container
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("ApplicationSettings"));

// Register the ApplicationDbContext as a service with options to use SQL Server, pulling the connection string from appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services to the application. Configures password policies, uniqueness of email, etc.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    // Additional configuration options can be set here
})
.AddEntityFrameworkStores<ApplicationDbContext>() // Specifies that the EF DbContext will store Identity information
.AddDefaultTokenProviders(); // Adds providers for generating tokens for reset passwords, two-factor authentication, etc.

var applicationSettings = builder.Configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>()
   ?? throw new NullReferenceException("ApplicationSettings cannot be null.");

// Adds JWT authentication to the application, configuring it with parameters from ApplicationSettings
builder.Services
    .AddJwtAuthentication(applicationSettings.JwtSigningKey, applicationSettings.JwtIssuer, applicationSettings.JwtAudience);

// Registers controllers as services
builder.Services.AddControllers();

// Configures Swagger/OpenAPI for API documentation and testing interface
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CodedByKay.BondBridge.API", Version = "v1" });
    // Defines the security scheme for authorization using JWT Bearer tokens
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    // Adds a global security requirement for the defined security scheme
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Build the WebApplication instance
var app = builder.Build();

// Configure Swagger UI if in development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CodedByKay.BondBridge.API V1");
    });
}

// Register custom middleware for global error handling and header validation
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<ValidateCustomHeaderMiddleware>();

// Create a minimal API endpoint to verify if the application is running
app.MapGet("/", () => "The application is running!");

// Middlewares to enforce HTTPS, handle authentication and authorization
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map controllers to endpoints
app.MapControllers();

// Run the application
app.Run();

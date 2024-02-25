using CodedByKay.BondBridge.API.Exstensions;
using CodedByKay.BondBridge.API.Models;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//Get and add Application settings to contaner
var applicationSettings = builder
    .Configuration.GetSection("ApplicationSettings")
    .Get<ApplicationSettings>()
    ?? throw new Exception("Application settings can not be null.");

builder.Services.AddSingleton(applicationSettings);

var jwtSigningKey = applicationSettings.JWTSIGNINGKEY;
var jwtIssuer = applicationSettings.JWTISSUER;
var jwtAudience = applicationSettings.JWTAUDIENCE;

// Add services to the container.
var services = builder.Services;

builder.Services
    .AddJwtAuthentication(jwtSigningKey, jwtIssuer, jwtAudience)
    .AddControllers();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CodedByKay.BondBridge.API", Version = "v1" });

    // Define the Bearer token authorization scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CodedByKay.BondBridge.API V1");
    });
}

app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.Run();

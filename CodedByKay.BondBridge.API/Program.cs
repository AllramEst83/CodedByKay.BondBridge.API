using CodedByKay.BondBridge.API.Models;
using CodedByKay.BondBridge.JwtAuth;

var builder = WebApplication.CreateBuilder(args);

//Get and add Application settings to contaner
var applicationSettings = builder.Configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>();
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
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.Run();

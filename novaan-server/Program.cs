using FileServer;
using Microsoft.IdentityModel.Tokens;
using MongoConnector;
using NovaanServer.Auth;
using NovaanServer.Developer;
using NovaanServer.ExceptionLayer;
using NovaanServer.src.Content;
using NovaanServer.src.Auth.Jwt;
using S3Connector;
using System.Text;
using NovaanServer.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NovaanServer.src.Preference;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Config MongoDB + AWS S3 Service
builder.Services.AddSingleton<MongoDBService>();
builder.Services.AddSingleton<S3Service>();
builder.Services.AddSingleton<FileService>();

// Server services register
builder.Services.AddScoped<IDevService, DevService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IPreferenceService, PreferenceService>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwTConfig"));

var tokenSettings = GetTokenValidationParameters(builder);
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(jwt =>
    {
        jwt.SaveToken = true;
        jwt.TokenValidationParameters = tokenSettings;
    });

builder.Services.AddSingleton<TokenValidationParameters>(tokenSettings);

var app = builder.Build();

await SeedData(app);

app.UseMiddleware<ExceptionFilter>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static TokenValidationParameters GetTokenValidationParameters(WebApplicationBuilder builder)
{
    var jwtSecret = builder.Configuration.GetSection("JwTConfig:Secret");
    if (!jwtSecret.Exists() || jwtSecret.Value == null)
    {
        throw new Exception("JwtConfig:JwtSecret not configured in appsettings.json");
    }

    var key = Encoding.ASCII.GetBytes(jwtSecret.Value);
    return new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false, // for dev
        ValidateAudience = false, // for dev
        RequireExpirationTime = false, // need to update when refresh token implement
        ValidateLifetime = true,
    };
}

static async Task SeedData(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var mongoDBService = services.GetRequiredService<MongoDBService>();
        mongoDBService.PingDatabase();
        await mongoDBService.SeedDietData();
        await mongoDBService.SeedCuisineData();
        await mongoDBService.SeedMealTypeData();
        await mongoDBService.SeedAllergenData();
    }
}



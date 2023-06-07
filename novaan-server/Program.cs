using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoConnector;
using NovaanServer.Auth;
using NovaanServer.Configuration;
using NovaanServer.Developer;
using NovaanServer.ExceptionLayer;
using NovaanServer.src.Auth.Jwt;
using S3Connector;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<MongoDBService>();
builder.Services.AddSingleton<S3Service>();

builder.Services.AddScoped<IDevService, DevService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwTConfig"));

var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwTConfig:Secret").Value);

var tokenValidationParameter = new TokenValidationParameters()
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = false, // for dev
    ValidateAudience = false, // for dev
    RequireExpirationTime = false, // need to update when refresh token implement
    ValidateLifetime = true,

};

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(jwt =>
    {     
        jwt.SaveToken = true;
        jwt.TokenValidationParameters = tokenValidationParameter;
    });

builder.Services.AddSingleton(tokenValidationParameter);
builder.Services.AddSingleton<JwtService>();

var app = builder.Build();

app.UseMiddleware<ExceptionFilter>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


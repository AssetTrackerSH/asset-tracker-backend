using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PortfolioTracker.Api.Endpoints;
using PortfolioTracker.Application;
using PortfolioTracker.Infrastructure;
using PortfolioTracker.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// JwtSettings'i configuration'dan okuyup DI'a kaydediyoruz.
// Bu sayede herhangi bir servis IOptions<JwtSettings> inject edip değerlere ulaşabilir.
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

// JWT Authentication middleware'ini ekliyoruz.
// AddAuthentication: varsayılan kimlik doğrulama şeması JwtBearer olsun.
// AddJwtBearer: gelen token'ları nasıl doğrulayacağımızı tanımlıyoruz.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
        };
    });

// [Authorize] attribute'unun çalışması için gerekli.
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Sıralama şart: önce kim olduğunu öğren, sonra ne yapabileceğine bak.
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapPricesEndpoints();
app.MapAuthEndpoints();

app.Run();

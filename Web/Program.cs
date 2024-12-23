using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AuthService.Handler;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using AuthService.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Настройка CORS
builder.Services.AddCors(options => 
{ options.AddPolicy("AllowAll", builder => 
    { 
        builder.AllowAnyOrigin() 
        .AllowAnyMethod() 
        .AllowAnyHeader(); 
    }); 
});

// Регистрация DbContext
var connectionString = "Host=localhost;Port=5432;Database=users;Username=postgres;Password=1;Timeout=10;SslMode=Disable";
builder.Services.AddDbContext<DBC>(options => options.UseNpgsql(connectionString));

// Добавление аутентификации и JWT
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
        ValidIssuer = "yourIssuer",
        ValidAudience = "yourAudience",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("verysecretverysecretverysecretkeykeykey"))
    };
});
builder.Services.AddSwaggerGen(options => 
{ 
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme 
    { 
        In = ParameterLocation.Header, 
        Description = "Please enter JWT with Bearer into field", 
        Name = "Authorization", 
        Type = SecuritySchemeType.ApiKey, 
        Scheme = "Bearer" 
    }); 
    options.AddSecurityRequirement(new OpenApiSecurityRequirement 
    { 
        {
            new OpenApiSecurityScheme 
            {
                Reference = new OpenApiReference 
                { 
                    Type = ReferenceType.SecurityScheme, Id = "Bearer"
                } 
            },
            new string[] { } 
        } 
    }); 
});
builder.Services.AddSwaggerGen(c => 
{ 
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "AuthorizationService", 
        Version = "v1.0",
        Description = "Проект представляет собой сервис для авторизации и переавторизации пользователя"
    }); 
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.MapControllers();

app.Run("http://localhost:5433");


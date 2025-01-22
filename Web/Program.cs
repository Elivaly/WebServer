using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AuthService.Handler;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using AuthService.Exceptions;
using AuthService.Middleware;
using System.Reflection;
using AuthService.Interface;
using AuthService.Service;

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
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
builder.Services.AddDbContext<DBC>(options => options.UseNpgsql(connectionString));

// Попытка вернуть куки
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

var key = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(key))
{ 
    throw new ArgumentNullException(nameof(key), "JWT Key cannot be null or empty."); 
}

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
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
        Version = "v1.2",
        Description = "Проект представляет собой серверную часть для авторизации и переавторизации пользователя"
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"; 
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile); 
    c.IncludeXmlComments(xmlPath);
    c.EnableAnnotations();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IRabbitSenderService, RabbitSenderService>();
builder.Services.AddScoped<IRabbitListenerService, RabbitListenerService>();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseCors("AllowAll");

//app.UseHttpsRedirection();
app.UseMiddleware<TokenValidateMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run(builder.Configuration["ApplicationHost:Address"]);


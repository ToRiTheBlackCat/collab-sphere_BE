using Amazon.S3;
using CloudinaryDotNet;
using CollabSphere.API.Hubs;
using CollabSphere.Application;
using CollabSphere.Application.Common;
using CollabSphere.Domain;
using CollabSphere.Infrastructure;
using CollabSphere.Infrastructure.Base;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Configure JsonIgnoreCycles
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Always;
});
#endregion

builder.Services.AddEndpointsApiExplorer();

#region Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "COLLAB-SPHERE_API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Example: \"Bearer {token}\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
#endregion

#region Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.WithOrigins(
                "null",
                "http://127.0.0.1:5500",
                "http://52.221.106.143/"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

#endregion

#region Configure JWT Authentication & Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"] ?? "")),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
    };

    //Configure using for Hubs
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/KanbanServiceHub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
#endregion

#region Configure Serilog
var solutionRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
var logFolder = Path.Combine(Environment.CurrentDirectory, "Loggings");
Directory.CreateDirectory(logFolder);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(logFolder, "log-.txt"), rollingInterval: RollingInterval.Day)
    .CreateLogger();


builder.Host.UseSerilog();
#endregion

#region Register Modules 

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);
#endregion

#region Register Base
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<JWTAuthentication>();
builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddMemoryCache();
#endregion

#region Configure Cloudinary
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddSingleton(provider =>
{
    var settings = provider.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    return new Cloudinary(new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret));
});
#endregion

#region Configure ExcelParser
builder.Services.AddScoped<IExcelFormatValidator, ValidateTableFormat>();
#endregion

#region Configure Redis (Upstash)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisSection = builder.Configuration.GetSection("Redis");
    var redisUrl = redisSection["RedisUrl"] ?? "";

    var options = ConfigurationOptions.Parse(redisUrl);
    options.AbortOnConnectFail = false;

    return ConnectionMultiplexer.Connect(options);
});

// Optional: add a wrapper service for easy usage
builder.Services.AddScoped<IDatabase>(sp =>
{
    var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
    return multiplexer.GetDatabase();
});
#endregion

#region Register Aws S3 Client
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
#endregion

#region Configure Hubs
builder.Services.AddSignalR();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "COLLAB-SPHERE_API v1");
    c.RoutePrefix = "swagger"; // so Swagger UI is at /swagger
});


app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//Hubs
app.MapHub<KanbanHub>("/KanbanServiceHub");
app.MapControllers();

app.Run();

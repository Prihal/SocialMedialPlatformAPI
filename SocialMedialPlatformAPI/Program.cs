using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocialMedialPlatformAPI.BLL;
using SocialMedialPlatformAPI.Common;
using SocialMedialPlatformAPI.Data;
using SocialMedialPlatformAPI.DTO;
using SocialMedialPlatformAPI.Helpers;
using SocialMedialPlatformAPI.Interface;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.OpenApi.Models;
using SocialMedialPlatformAPI.Hubs;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        //Add ConnectionString On Database
        var provider = builder.Services.BuildServiceProvider();
        var config = provider.GetRequiredService<IConfiguration>();
        builder.Services.AddDbContext<AppDbContext>(op => op.UseSqlServer(config.GetConnectionString("con")));

        // Register IHttpContextAccessor
        builder.Services.AddHttpContextAccessor();

        // Configure authentication
        var key = builder.Configuration.GetValue<string>("Jwt:Key") ?? throw new InvalidOperationException("JWT Key is not configured.");

        //Add Google Authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax; // Adjust as needed
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Ensure cookies are sent over HTTPS if using HTTPS
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        (path.StartsWithSegments("/notificationHub") || path.StartsWithSegments("/chatHub")))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        })
        .AddGoogle(options =>
        {
            options.ClientId = builder.Configuration["Google:ClientId"];
            options.ClientSecret = builder.Configuration["Google:ClientSecret"];
            options.CallbackPath = "/api/google-response";
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });



        //AddScope
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IValidationService, ValidationService>();
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<Helper>();
        builder.Services.AddScoped<ResponseHandler>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<NotificationHub>();
        builder.Services.AddScoped<IPostService, PostService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<IChatService, ChatServicecs>();
        builder.Services.AddScoped<IStoryServicecs, StoryServices>();

        // Add SignalR for hubs
        builder.Services.AddSignalR();

        // Configure JSON serialization settings
        builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            })
             .AddJsonOptions(options =>
             {
                 options.JsonSerializerOptions.IgnoreNullValues = true; // Optional, ignore null values
             });

        // Configure CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });


        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                              "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                              "Example: \"12345abcdef\"",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Scheme = "Bearer"
                
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                In = ParameterLocation.Header
            },
            new List<string>()
        }
            });
        });

        var app = builder.Build();


        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCors("AllowAll");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<NotificationHub>("/notificationHub");


        app.Run();
    }
}
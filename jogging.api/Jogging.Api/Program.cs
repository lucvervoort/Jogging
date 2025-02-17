//using AutoMapper;
using Jogging.Api.Configuration;
//using Jogging.Domain.Configuration;
//using Jogging.Domain.Helpers;
using Jogging.Persistence.Context;
using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Diagnostics;
//using Microsoft.Extensions.Configuration;
using Serilog;
//using Serilog.Sinks.Discord;

namespace Jogging.Api;

internal class Program
{
    public static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        /*
        var discordInformation = configuration.GetSection("Discord").Get<DiscordConfiguration>();

        if (discordInformation == null || discordInformation.WebhookId == 0 || string.IsNullOrWhiteSpace(discordInformation.WebhookToken))
        {
            throw new ApplicationException("Discord configuration is missing or invalid. Please check your WebhookId and WebhookToken.");
        }
        */

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            //.WriteTo.Discord(discordInformation.WebhookId, discordInformation.WebhookToken)
            .CreateLogger();

        try
        {
            Log.Information("Starting web host...");

            var builder = WebApplication.CreateBuilder(args);

            // Configure MySQL database connection
            builder.Services.AddDbContext<JoggingContext>(options => {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                if(string.IsNullOrEmpty(connectionString))
                {
                    connectionString = "server=docker.host.internal;port=3306;database=jogging2;user=root;password=root";
                }
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                options.LogTo(Log.Warning, LogLevel.Warning);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            builder.Services.AddMemoryCache();

            // AutoMapper configuration
            builder.Services.AddAutoMapper(typeof(MappingConfig));

            // Configure route options for lowercase URLs
            builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            // Add API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddControllers()
                .AddJsonOptions(options => {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                });

            // Add other services (MultiSafepay, Email client, etc.)
            builder.Services.AddMultiSafepay(configuration);
            builder.Services.AddSmtpEmailClient(configuration);
            builder.Services.AddInterfaces();
            builder.Services.AddDomainManagerServices();
            builder.Services.AddHelperServices();

            // CORS Configuration
            var corsOptions = builder.Configuration.GetSection("Cors").Get<JoggingCorsOptions>();
            if (corsOptions != null)
            {
                Log.Information("Configured CORS in appsettings.json");
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowSpecificOrigin", policyBuilder =>
                    {
                        policyBuilder.WithOrigins(corsOptions.AllowedOrigins.Split(','))
                                .WithMethods(corsOptions.AllowedMethods.Split(','))
                                .WithHeaders(corsOptions.AllowedHeaders.Split(','))
                                .WithExposedHeaders("X-Pagination".Split(','))
                                .AllowCredentials(); // Allow credentials (cookies, authorization headers, etc.)
                    });
                });
            }
            else
            {
                Log.Information("Hardcoded CORS");
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowSpecificOrigin", policyBuilder =>
                    {
                        policyBuilder.WithOrigins(
                            "http://localhost:8888",
                            "http://localhost:5187",
                            "https://localhost:7073"
                            ) // Replace with your actual frontend URL
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .WithExposedHeaders("X-Pagination")
                            .AllowCredentials(); // Allow credentials (cookies, authorization headers, etc.)
                    });
                });
            }

            // Configure Serilog for logging
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            // Rate Limiting
            builder.Services.AddRateLimiter(RateLimiterConfigurator.ConfigureRateLimiter);
            builder.Services.AddXFrameSupress();

            // Authentication and Swagger
            builder.Services.AddCustomAuthentication(configuration);
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Add HSTS headers
            HelmetConfig.AddHsts(app, builder);

            // Enable Swagger UI only in development
            // if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "jogging-api v1"));
            }

            // Apply rate limiter and helmet headers
            app.UseRateLimiter();
            app.UseHelmetHeaders();

            // Use CORS policy
            app.UseCors("AllowSpecificOrigin");  // Apply the new CORS policy

            // Authentication and Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Custom middlewares for JWT validation and rate limiting
            app.UseMiddleware<JwtTokenValidationMiddleware>();
            app.UseMiddleware<RateLimitingMiddleware>("/api/auth/request-confirm-mail");

            // Map controllers and run the application
            app.MapControllers();
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

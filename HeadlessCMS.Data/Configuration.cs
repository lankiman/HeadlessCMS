using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace HeadlessCMS.Data;

public static class Configuration
{

    public static IServiceCollection AddDataServices(
        this IServiceCollection services, 
        IConfiguration configuration,
        IHostEnvironment environment) 
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Stores and reads migrations inside HeadlessCMS.Data
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);

                // Set command timeout to 30 seconds
                npgsqlOptions.CommandTimeout(30);

                // Enable resilient connection retries for transient cloud errors (e.g. Railway DB startup delays)
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            });
            
            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });
        var redisConnectionString = configuration.GetConnectionString("Redis") 
            ?? "localhost:6379"; // Fallback to local default if missing
        
        services.AddSingleton<IConnectionMultiplexer>(sp => 
        {
            var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
            configurationOptions.AbortOnConnectFail = false;
    
            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            
        });
        
        return services;
    }
    
    public static async Task InitDataServicesAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        // 1. Check Redis Connection
        try
        {
            var redis = services.GetRequiredService<IConnectionMultiplexer>();
            var pingTime = await redis.GetDatabase().PingAsync();
            Console.WriteLine($"✅ Connected to Redis! Ping: {pingTime.TotalMilliseconds}ms");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Redis Connection Failed: {ex.Message}");
        }

        // 2. Ensure DB Exists and Apply Migrations
        try
        {
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            
            // Creates the DB if missing + creates all tables/migrations automatically
            await dbContext.Database.MigrateAsync();
            
            Console.WriteLine($"✅ PostgreSQL Database verified and up to date!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ PostgreSQL Database Setup Failed: {ex.Message}");
        }
    }
}

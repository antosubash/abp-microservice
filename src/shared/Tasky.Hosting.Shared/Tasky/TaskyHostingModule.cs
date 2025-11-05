using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using StackExchange.Redis;
using Tasky.MultiTenancy;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs.RabbitMQ;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Data;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.RabbitMQ;
using Volo.Abp.Swashbuckle;

namespace Tasky;

internal sealed class NoOpDistributedLockProvider : IDistributedLockProvider
{
    public IDistributedLock CreateLock(string name)
    {
        return new NoOpDistributedLock();
    }

    private sealed class NoOpDistributedLock : IDistributedLock
    {
        public string Name => string.Empty;

        public IDistributedSynchronizationHandle? TryAcquire(TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return new NoOpDistributedLockHandle();
        }

        public IDistributedSynchronizationHandle Acquire(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            return new NoOpDistributedLockHandle();
        }

        public ValueTask<IDistributedSynchronizationHandle?> TryAcquireAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<IDistributedSynchronizationHandle?>(new NoOpDistributedLockHandle());
        }

        public ValueTask<IDistributedSynchronizationHandle> AcquireAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<IDistributedSynchronizationHandle>(new NoOpDistributedLockHandle());
        }

        private sealed class NoOpDistributedLockHandle : IDistributedSynchronizationHandle
        {
            public CancellationToken HandleLostToken => CancellationToken.None;

            public void Dispose() { }

            public ValueTask DisposeAsync()
            {
                return ValueTask.CompletedTask;
            }
        }
    }
}

[DependsOn(typeof(AbpAspNetCoreMultiTenancyModule))]
[DependsOn(typeof(AbpAspNetCoreSerilogModule))]
[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(AbpBackgroundJobsRabbitMqModule))]
[DependsOn(typeof(AbpCachingStackExchangeRedisModule))]
[DependsOn(typeof(AbpDataModule))]
[DependsOn(typeof(AbpDistributedLockingModule))]
[DependsOn(typeof(AbpEventBusRabbitMqModule))]
[DependsOn(typeof(AbpSwashbuckleModule))]
[DependsOn(typeof(TaskySharedModule))]
public class TaskyHostingModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        var isTestEnvironment = hostingEnvironment.IsEnvironment("Test") || 
                               configuration["ASPNETCORE_ENVIRONMENT"] == "Test" ||
                               configuration["Environment"] == "Test";

        // Configure in-memory distributed cache for test environment when Redis is not available
        if (isTestEnvironment)
        {
            var redisConnectionString = configuration.GetConnectionString(TaskyNames.Redis);
            if (string.IsNullOrWhiteSpace(redisConnectionString))
            {
                context.Services.AddDistributedMemoryCache();
            }
        }

        ConfigureDistributedLocking(context, configuration);

        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = MultiTenancyConsts.IsEnabled;
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("ar", "ar", "العربية"));
            options.Languages.Add(new LanguageInfo("cs", "cs", "Čeština"));
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
            options.Languages.Add(new LanguageInfo("en-GB", "en-GB", "English (UK)"));
            options.Languages.Add(new LanguageInfo("fi", "fi", "Finnish"));
            options.Languages.Add(new LanguageInfo("fr", "fr", "Français"));
            options.Languages.Add(new LanguageInfo("hi", "hi", "Hindi"));
            options.Languages.Add(new LanguageInfo("is", "is", "Icelandic"));
            options.Languages.Add(new LanguageInfo("it", "it", "Italiano"));
            options.Languages.Add(new LanguageInfo("hu", "hu", "Magyar"));
            options.Languages.Add(new LanguageInfo("pt-BR", "pt-BR", "Português"));
            options.Languages.Add(new LanguageInfo("ro-RO", "ro-RO", "Română"));
            options.Languages.Add(new LanguageInfo("ru", "ru", "Русский"));
            options.Languages.Add(new LanguageInfo("sk", "sk", "Slovak"));
            options.Languages.Add(new LanguageInfo("tr", "tr", "Türkçe"));
            options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "简体中文"));
            options.Languages.Add(new LanguageInfo("zh-Hant", "zh-Hant", "繁體中文"));
            options.Languages.Add(new LanguageInfo("de-DE", "de-DE", "Deutsch"));
            options.Languages.Add(new LanguageInfo("es", "es", "Español"));
        });

        var rabbitMqConnectionString = configuration.GetConnectionString(TaskyNames.RabbitMq);
        
        if (!string.IsNullOrWhiteSpace(rabbitMqConnectionString) && !isTestEnvironment)
        {
            Configure<AbpRabbitMqOptions>(options =>
            {
                options.Connections.Default = new ConnectionFactory() { Uri = new Uri(rabbitMqConnectionString) };
            });

            var clientName = configuration["RabbitMQ:EventBus:ClientName"];
            var exchangeName = configuration["RabbitMQ:EventBus:ExchangeName"];
            if (!string.IsNullOrWhiteSpace(clientName) && !string.IsNullOrWhiteSpace(exchangeName))
            {
                Configure<AbpRabbitMqEventBusOptions>(options =>
                {
                    options.ClientName = clientName;
                    options.ExchangeName = exchangeName;
                });
            }
        }
        else if (isTestEnvironment && !string.IsNullOrWhiteSpace(rabbitMqConnectionString))
        {
            // In test environment with a connection string, configure RabbitMQ with minimal timeouts
            Configure<AbpRabbitMqOptions>(options =>
            {
                if (options.Connections.Default == null)
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = "127.0.0.1",
                        Port = 5672,
                        RequestedConnectionTimeout = TimeSpan.FromMilliseconds(1),
                        SocketReadTimeout = TimeSpan.FromMilliseconds(1),
                        SocketWriteTimeout = TimeSpan.FromMilliseconds(1),
                        NetworkRecoveryInterval = TimeSpan.Zero,
                        AutomaticRecoveryEnabled = false
                    };
                    options.Connections.Default = factory;
                }
            });
        }
    }

    private static void ConfigureDistributedLocking(
        ServiceConfigurationContext context,
        IConfiguration configuration
    )
    {
        var redisConnectionString = configuration.GetConnectionString(TaskyNames.Redis);
        if (!string.IsNullOrWhiteSpace(redisConnectionString) && !redisConnectionString.Contains(":0"))
        {
            context.Services.AddSingleton<IDistributedLockProvider>(sp =>
            {
                var connection = ConnectionMultiplexer.Connect(redisConnectionString);
                return new RedisDistributedSynchronizationProvider(connection.GetDatabase());
            });
        }
        else
        {
            context.Services.AddSingleton<IDistributedLockProvider>(
                sp => new NoOpDistributedLockProvider()
            );
        }
    }
}

public static class HostingExtensions
{
    public static ServiceConfigurationContext ConfigureDataProtection(
        this ServiceConfigurationContext context,
        IWebHostEnvironment hostingEnvironment,
        IConfiguration configuration,
        string name
    )
    {
        var dataProtectionBuilder = context
            .Services.AddDataProtection()
            .SetApplicationName(TaskyNames.Tasky);
        if (!hostingEnvironment.IsDevelopment())
        {
            var redisConnectionString = configuration.GetConnectionString(TaskyNames.Redis);
            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                var redis = ConnectionMultiplexer.Connect(redisConnectionString);
                dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, $"{name}-Keys");
            }
        }

        return context;
    }
}

using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Microsoft.Extensions.Hosting;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddSharedEndpoints(this IHostApplicationBuilder builder)
    {
        var isTestEnvironment = builder.Configuration["ASPNETCORE_ENVIRONMENT"] == "Test" ||
                               builder.Configuration["Environment"] == "Test";
        
        var rabbitMqConnectionString = builder.Configuration.GetConnectionString(TaskyNames.RabbitMq);
        if (!string.IsNullOrWhiteSpace(rabbitMqConnectionString))
        {
            builder.AddRabbitMQClient(
                connectionName: TaskyNames.RabbitMq,
                action =>
                    action.ConnectionString = rabbitMqConnectionString
            );
        }
        
        var redisConnectionString = builder.Configuration.GetConnectionString(TaskyNames.Redis);
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            if (isTestEnvironment)
            {
                // In test environment, configure Redis cache manually to bypass Aspire connection management
                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                });
            }
            else
            {
                builder.AddRedisDistributedCache(connectionName: TaskyNames.Redis);
            }
        }
        
        var seqConnectionString = builder.Configuration.GetConnectionString(TaskyNames.Seq);
        if (!string.IsNullOrWhiteSpace(seqConnectionString))
        {
            builder.AddSeqEndpoint(connectionName: TaskyNames.Seq);
        }

        return builder;
    }
}

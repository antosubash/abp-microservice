namespace Microsoft.Extensions.Hosting;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddSharedEndpoints(this IHostApplicationBuilder builder)
    {
        // RabbitMQ client is configured through ABP's RabbitMQ integration (Volo.Abp.RabbitMQ)
        // No need for Aspire.RabbitMQ.Client as ABP handles RabbitMQ connections
        builder.AddRedisDistributedCache(connectionName: TaskyNames.Redis);
        builder.AddSeqEndpoint(connectionName: TaskyNames.Seq);

        return builder;
    }
}

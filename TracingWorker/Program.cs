using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using TracingWorker;
using TracingWorker.Infrastructure;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddDbContext<PersistenceContext>(opts =>
        {
            opts.UseInMemoryDatabase("sampleDb");
        }, contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton);

        services.AddHostedService<Worker>();
        services.AddMediatR(typeof(Program));
        services.AddSingleton<IConnection>(svc =>
        {
            var connFactory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest",
                Port = 5672
            };
            return connFactory.CreateConnection();
        });

        var serviceName = "Worker";     

        services.AddOpenTelemetryTracing((builder) =>
        {
            builder.AddSource(serviceName)
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
            .AddHttpClientInstrumentation()
            .AddSqlClientInstrumentation(opts =>
            {
                opts.SetDbStatementForText = true;
                opts.RecordException = true; 
                opts.EnableConnectionLevelAttributes = true;
                opts.RecordException = true;
            })
            .AddZipkinExporter(zipkinOptions =>
            {
                zipkinOptions.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
            })
            .AddConsoleExporter();
        });

    })
    .Build();

await host.RunAsync();

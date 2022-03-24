using MediatR;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using TracingApi.Application.Commands;
using TracingApi.Application.Ports;
using TracingApi.Infrastructure.Adapters;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(typeof(Program));
var serviceName = "ApiEntryPoint";

builder.Services.AddOpenTelemetryTracing((builder) =>
{
    builder.AddSource(serviceName)
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
    .AddAspNetCoreInstrumentation(opts =>
    {
        opts.Filter = (req) => !req.Request.Path.ToUriComponent().Contains("swagger");       
    })    
    .AddZipkinExporter(zipkinOptions =>
    {
        zipkinOptions.Endpoint = new Uri(config.GetValue<string>("zipkinUrl"));
    })
    .AddConsoleExporter();
});

builder.Services.AddSingleton<IConnection>(svc =>
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

builder.Services.AddTransient<IMessaging, RabbitMessaging>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("api/person", async (IMediator mediator, CreatePersonCommand request) => await mediator.Send(request));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

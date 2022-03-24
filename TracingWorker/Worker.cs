using System.Diagnostics;
using System.Text;
using System.Text.Json;
using MediatR;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TracingWorker.Application.Commands;
using TracingWorker.Domain.Entities;

namespace TracingWorker;

public class Worker : BackgroundService, IDisposable
{
    private static readonly ActivitySource ActivitySource = new ActivitySource("Worker");
    private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

    readonly ILogger<Worker> _logger;        
    readonly IModel _model;
    readonly IMediator _mediator;

    bool _disposed;
    readonly string _queueName;
    const string ACTIVITY_NAME = "Launching handler to process request";

    public Worker(ILogger<Worker> logger, IConnection conn, IConfiguration config, IMediator mediator)
    {
        _logger = logger;                
        _model = conn.CreateModel();
        _queueName = config.GetValue<string>("targetQueue");
        _disposed = false;
        _mediator = mediator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();
                           
        _model.QueueDeclare(_queueName, false, false, false, null);

        var consumer = new EventingBasicConsumer(_model);

        consumer.Received += async (sender, e) =>
        {
            var parentContext = Propagator.Extract(default, e.BasicProperties, ExtractHeaders);
            Baggage.Current = parentContext.Baggage;

            using (var activity = ActivitySource.StartActivity(ACTIVITY_NAME, ActivityKind.Consumer, parentContext.ActivityContext))
            {
                var payload = JsonSerializer.Deserialize<Person>(Encoding.UTF8.GetString(e.Body.Span.ToArray()));
                if (payload is not null)
                {
                    await _mediator.Send(new CreatePersonAsyncCommand(payload.FirstName, payload.LastName, payload.Email));
                }
            }

            _model.BasicAck(e.DeliveryTag, false);
        };

        _model.BasicConsume(_queueName, false, consumer);

        await Task.CompletedTask;
    }

    private IEnumerable<string> ExtractHeaders(IBasicProperties prop, string key)
    {
        try
        {
            if (prop.Headers.TryGetValue(key, out var value))
            {
                var bytes = value as byte[] ?? throw new Exception("no value");
                return new[] { System.Text.Encoding.UTF8.GetString(bytes) };
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Error extracting context", e);
        }

        return Enumerable.Empty<string>();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _model.Dispose();
        }

        _disposed = true;
        base.Dispose();
    }
}

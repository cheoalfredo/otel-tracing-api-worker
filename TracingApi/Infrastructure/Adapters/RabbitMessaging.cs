using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using System.Diagnostics;
using TracingApi.Application.Ports;

namespace TracingApi.Infrastructure.Adapters;

public class RabbitMessaging : IMessaging
{
    readonly IConnection _conn;
    readonly ILogger _logger;
    private static readonly ActivitySource _activitySource = new ActivitySource("ApiEntryPoint");
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;


    public RabbitMessaging(IConnection conn, ILogger<RabbitMessaging> log)
    {
        _conn = conn;
        _logger = log;
    }

    public async Task SendMessage(string Message, string Queue)
    {
        await Task.Run(() =>
        {
            using (var activity = _activitySource.StartActivity("Queue user creation request", ActivityKind.Producer))
            {
                using (var channel = _conn.CreateModel())
                {
                    channel.QueueDeclare(Queue, false, false, false, null);

                    ActivityContext contextToInject = default;

                    if (activity is not null)
                    {
                        contextToInject = activity.Context;

                    }
                    else if (Activity.Current is not null)
                    {
                        contextToInject = Activity.Current.Context;
                    }

                    var props = channel.CreateBasicProperties();

                    var propagationContext = new PropagationContext(contextToInject, Baggage.Current);

                    Propagator.Inject(propagationContext, props, InjectHeaders);                    

                    channel.BasicPublish("", Queue, props, System.Text.Encoding.UTF8.GetBytes(Message));
                }
            }
        });
    }
    private void InjectHeaders(IBasicProperties target, string key, string value)
    {
        try
        {
            if (target.Headers is null)
            {
                target.Headers = new Dictionary<string, object>();
            }

            target.Headers[key] = value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to inject trace context.");
        }
    }

}
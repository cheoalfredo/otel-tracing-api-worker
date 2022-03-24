using MediatR;
using System.ComponentModel.DataAnnotations;
using TracingApi.Application.Ports;

namespace TracingApi.Application.Commands;

public record CreatePersonCommand([Required] string FirstName, [Required] string LastName, [EmailAddress] string Email) : IRequest;

public class CreatePersonCommandHandler : AsyncRequestHandler<CreatePersonCommand>
{
    readonly IMessaging _msg;
    readonly string _queueName;
    public CreatePersonCommandHandler(IMessaging msg, IConfiguration config)
    {
        _msg = msg;
        _queueName = config.GetValue<string>("targetQueue");
    }
    protected override async Task Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        await _msg.SendMessage(System.Text.Json.JsonSerializer.Serialize(request), _queueName);
    }
}
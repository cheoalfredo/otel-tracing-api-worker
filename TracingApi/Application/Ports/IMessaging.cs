namespace TracingApi.Application.Ports;

public interface IMessaging {
    public Task SendMessage(string Message, string Queue);

}
using Microsoft.AspNetCore.SignalR;
public class Myhub : Hub
{
    public async Task Sendmsg(object body)
    {
        await Clients.All.SendAsync("ReceiveMessage", body);
    }
}
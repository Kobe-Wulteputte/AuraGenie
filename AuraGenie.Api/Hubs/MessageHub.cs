using AuraGenie.Data.Models;
using Microsoft.AspNetCore.SignalR;

namespace AuraGenie.Api.Hubs;

public class MessageHub : Hub
{
    private const string GroupName = "GenieChat";

    public override async Task OnConnectedAsync()
    {
        var aspNetUserId = Context.User?.Claims.FirstOrDefault(x => x.Type == "name")?.Value;
        Console.WriteLine("User connected: " + aspNetUserId);
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);

        await base.OnConnectedAsync();
    }
}
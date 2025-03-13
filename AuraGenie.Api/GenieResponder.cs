using AuraGenie.Api.Business;
using AuraGenie.Api.Helper;
using AuraGenie.Api.Hubs;
using AuraGenie.Data.Models;
using Microsoft.AspNetCore.SignalR;

namespace AuraGenie.Api;

public class GenieResponder(IServiceProvider sp, IHubContext<MessageHub> hubContext) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = sp.CreateScope();
        var cs = scope.ServiceProvider.GetRequiredService<ChatService>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var incomingMessage = await ChatService.Channel.Reader.ReadAsync(stoppingToken);
                var shouldRespond = cs.ShouldRespond(incomingMessage);
                if (!shouldRespond) continue;

                var spam = await cs.CheckSpam(incomingMessage);
                if (spam)
                {
                    var message = new Message
                    {
                        SenderId = "Genie",
                        MessageContent = "Je hebt al voldoende berichten gestuurd. Dit begint al genoeg te kosten.",
                        CreatedOn = DateTime.UtcNow.ToUnixTime(),
                        RoomId = incomingMessage.RoomId
                    };
                    await cs.SaveMessage(message);
                    continue;
                }

                await hubContext.Clients.Group("GenieChat").SendAsync("GenieTyping", true, cancellationToken: stoppingToken);
                // delay to simulate typing
                await Task.Delay(2000, stoppingToken);
                await cs.RespondToMessage(incomingMessage);
                await hubContext.Clients.Group("GenieChat").SendAsync("GenieTyping", false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
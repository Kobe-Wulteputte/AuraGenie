using System.Threading.Channels;
using AuraGenie.Api.Helper;
using AuraGenie.Api.Hubs;
using AuraGenie.Data.Context;
using AuraGenie.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AuraGenie.Api.Business;

public class ChatService(
    AuraContext ctx,
    OpenAiService aiService,
    IHttpContextAccessor contextAccessor,
    IConfiguration configuration,
    IHubContext<MessageHub> hubContext
)
{
    public static readonly Channel<Message> Channel = System.Threading.Channels.Channel.CreateUnbounded<Message>();

    public async Task<List<Message>> GetMessages(int roomId, int skip = 0, int take = 100)
    {
        var messages = await ctx.Messages
            .Include(x => x.ReplyMessage)
            .Where(m => m.RoomId == roomId)
            .OrderByDescending(x => x.CreatedOn)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return messages.OrderBy(x => x.CreatedOn).ToList();
    }

    public async Task AddMessage(Message message)
    {
        var sender = contextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        message.SenderId = sender ?? message.SenderId;
        message.MessageContent = message.MessageContent.Length > 1000 ? message.MessageContent[..1000] : message.MessageContent;
        message.CreatedOn = DateTime.UtcNow.ToUnixTime();
        message.RoomId = 1;
        if (string.IsNullOrEmpty(message.MessageContent)) return;
        await SaveMessage(message);
        await Channel.Writer.WriteAsync(message);
    }

    public async Task RespondToMessage(Message message)
    {
        var response = await aiService.CreateResponseForMessage(message);
        await SaveMessage(response);
    }

    public async Task SaveMessage(Message message)
    {
        var replyMessage = message.ReplyMessage;
        message.ReplyMessage = null;
        ctx.Messages.Add(message);
        await ctx.SaveChangesAsync();
        message.ReplyMessage = replyMessage;
        await hubContext.Clients.Group("GenieChat").SendAsync("ReceiveMessage", message);
    }

    public async Task<bool> CheckSpam(Message incomingMessage)
    {
        var yesterday = DateTime.Now.AddDays(-1).ToUnixTime();
        var messageFromUserCount = await ctx.Messages
            .Where(m => m.SenderId == incomingMessage.SenderId &&
                        m.CreatedOn > yesterday &&
                        m.RoomId == incomingMessage.RoomId &&
                        m.AuraPointsLogs.Count > 0)
            .CountAsync();
        var maxDailyMessages = configuration.GetValue<int>("MaxDailyMessages");
        return messageFromUserCount > maxDailyMessages;
    }

    public bool ShouldRespond(Message incomingMessage)
    {
        var content = incomingMessage.MessageContent;
        if (string.IsNullOrWhiteSpace(content)) return false;
        return content.Contains("<usertag>Genie</usertag>", StringComparison.OrdinalIgnoreCase) ||
               content.Contains("@Genie", StringComparison.OrdinalIgnoreCase);
    }
}
using System.ClientModel;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AuraGenie.Api.Helper;
using AuraGenie.Data.Context;
using AuraGenie.Data.Models;
using Azure.AI.OpenAI;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;

namespace AuraGenie.Api.Business;

public class OpenAiService
{
    private readonly ChatClient _client;
    private readonly string _conversationContext = File.ReadAllText("prompt.txt");
    private readonly AuraContext _ctx;

    public OpenAiService(IConfiguration configuration, AuraContext ctx)
    {
        _ctx = ctx;
        var azureClient = new AzureOpenAIClient(new Uri(configuration["Azure:OpenAi:Url"]),
            new ApiKeyCredential(configuration["Azure:OpenAi:ApiKey"]));
        _client = azureClient.GetChatClient(configuration["Azure:OpenAi:ChatId"]);
    }

    private async Task<ChatCompletion> CreateRequestForMessage(Message m)
    {
        var scoreList = await GenerateScoreList();
        var updatedContext = _conversationContext.Replace("{{userList}}", await GenerateUserList());
        var scoreListMessage = $@"
De huidige scores zijn als volgt. Dit is al inclusief de voorbije gebeurtenissen.
Als naar een score stand gevraagd wordt, geef je altijd deze score terug. Als je antwoord op een gebeurtenis moet je wel de nieuwe score hier nog bijtellen.
Voor het optellen van scores mag je nooit naar een vorig bericht kijken. Bereken dit altijd opnieuw op basis van volgende data.
{scoreList}
";
        var thisMorning = DateTime.UtcNow.Date.ToUnixTime();
        var messagesOfToday = await _ctx.Messages.Where(x => x.RoomId == m.RoomId
                                                             && m.Id != x.Id
                                                             && x.CreatedOn < m.CreatedOn
                                                             && x.CreatedOn > thisMorning)
            .OrderByDescending(x => x.CreatedOn).Take(20)
            .ToListAsync();
        messagesOfToday = messagesOfToday.OrderBy(x => x.CreatedOn).ToList();
        var oldMessages = messagesOfToday.Select<Message, ChatMessage>(x =>
        {
            if (x.SenderId == "Genie")
            {
                return new AssistantChatMessage($"""
                                                 Verteller: {x.SenderId}
                                                 Bericht: {x.MessageContent}
                                                 """);
            }

            return new UserChatMessage($"""
                                        Verteller: {x.SenderId}
                                        Bericht: {x.MessageContent}
                                        """);
        }).ToList();

        var newUserMessage = $"""
                              Verteller: {m.SenderId}
                              Bericht: {m.MessageContent}
                              """;

        List<ChatMessage> messages =
        [
            new SystemChatMessage(updatedContext),
            ..oldMessages,
            new SystemChatMessage(scoreListMessage),
            new UserChatMessage(newUserMessage),
        ];
        var clientResult = await _client.CompleteChatAsync(
            messages,
            new ChatCompletionOptions()
            {
                StopSequences = { "Belgische straattaal", "stereotype gen-z humor", "Hoe je punten geeft" }
            }
        );
        return clientResult.Value;
    }


    private async Task<string> GenerateScoreList()
    {
        var pointsPerUser = await _ctx.AuraPointsLogs.GroupBy(x => x.UserId)
            .Select(x => new
            {
                UserId = x.Key,
                Points = x.Sum(x => x.Points)
            })
            .OrderByDescending(x => x.Points)
            .ToListAsync();
        var users = await _ctx.Users.ToListAsync();
        var sb = new StringBuilder();
        foreach (var ppu in pointsPerUser)
        {
            var user = users.FirstOrDefault(x => x.Id == ppu.UserId);
            if (user == null) continue;
            var points = pointsPerUser.FirstOrDefault(x => x.UserId == user.Id)?.Points ?? 0;
            sb.AppendLine($"{user.Username}: {points}");
        }

        return sb.ToString();
    }

    private async Task<string> GenerateUserList()
    {
        var users = await _ctx.Users.ToListAsync();
        var sb = new StringBuilder();
        foreach (var u in users)
        {
            sb.Append($"{u.Username} ,");
        }

        return sb.ToString();
    }

    public async Task<Message> CreateResponseForMessage(Message m)
    {
        var completion = await CreateRequestForMessage(m);
        var messageContent = completion.Content[0].Text;

        messageContent = messageContent.Replace("```json", "", StringComparison.OrdinalIgnoreCase);
        messageContent = messageContent.Replace("JSON:", "", StringComparison.OrdinalIgnoreCase);
        messageContent = messageContent.Replace("```", "");
        messageContent = messageContent.Replace("Verteller: Genie", "", StringComparison.OrdinalIgnoreCase);
        messageContent = messageContent.Replace("Bericht:", "", StringComparison.OrdinalIgnoreCase);
        messageContent = messageContent.Trim();

        // Extract json object
        var regex = new Regex(@"(.*?)\s*(\{.*\})", RegexOptions.Singleline);
        var match = regex.Match(messageContent);
        if (match.Success)
        {
            try
            {
                messageContent = match.Groups[1].Value;
                var json = match.Groups[2].Value;
                await SavePointsToLog(json, m);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        return new Message
        {
            MessageContent = messageContent,
            RoomId = m.RoomId,
            SenderId = "Genie",
            CreatedOn = DateTime.UtcNow.ToUnixTime(),
            ReplyMessageId = m.Id,
            ReplyMessage = m
        };
    }

    private async Task SavePointsToLog(string json, Message m)
    {
        var obj = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        if (obj == null) return;
        foreach (var (name, value) in obj)
        {
            if (value == null) continue;
            var score = int.Parse(value.ToString());
            if (score == 0) continue;
            var user = _ctx.Users.FirstOrDefault(x => x.Username.Contains(name));
            if (user == null) continue;
            var auraPointsLog = new AuraPointsLog
            {
                UserId = user.Id,
                Points = score,
                CreatedOn = DateTime.UtcNow.ToUnixTime(),
                SourceMessageId = m.Id
            };
            _ctx.AuraPointsLogs.Add(auraPointsLog);
        }

        await _ctx.SaveChangesAsync();
    }
}
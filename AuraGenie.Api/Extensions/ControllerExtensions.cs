using AuraGenie.Api.Business;
using AuraGenie.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace AuraGenie.Api.Extensions;

public static class ControllerExtensions
{
    public static void AddEndpoints(this WebApplication app)
    {
        app.MapGet("/rooms", async (RoomService rs) => await rs.GetAllRooms())
            .WithName("GetAllRooms")
            .RequireAuthorization(configure =>
            {
                configure.RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
            })
            .WithTags("Rooms");

        app.MapGet("/room/users", async ([FromQuery] int roomId, RoomService rs) => await rs.GetUsersInRoom(roomId))
            .WithName("GetUsersInRoom")
            .RequireAuthorization(configure =>
            {
                configure.RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
            })
            .WithTags("Rooms");

        app.MapGet("/chats", async ([FromQuery] int roomId, [FromQuery] int skip, [FromQuery] int take, ChatService cs)
                => await cs.GetMessages(roomId, skip, take))
            .WithName("ChatsByRoom")
            .RequireAuthorization(configure =>
            {
                configure.RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
            })
            .WithTags("Chats");

        app.MapPost("/chat", async ([FromBody] Message message, ChatService cs) => await cs.AddMessage(message))
            .WithName("SendMessage")
            .RequireAuthorization(configure =>
            {
                configure.RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
            })
            .WithTags("Chats");

        app.MapGet("/health", () => Results.Ok("Healthy!"))
            .WithName("HealthCheck")
            .WithTags("Health");
    }
}
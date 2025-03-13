using AuraGenie.Api.Helper;
using AuraGenie.Data.Context;
using AuraGenie.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AuraGenie.Api.Business;

public class RoomService(AuraContext ctx, IHttpContextAccessor contextAccessor)
{
    public Room? GetRoom(int id)
    {
        return ctx.Rooms.Find(id);
    }

    public async Task<List<User>> GetUsersInRoom(int roomId)
    {
        var users = await ctx.Users.ToListAsync();
        return users;
    }

    public async Task<List<Room>> GetAllRooms()
    {
        await CheckCurrentUserExists();
        return await ctx.Rooms.ToListAsync();
    }

    private async Task CheckCurrentUserExists()
    {
        var httpContext = contextAccessor.HttpContext;
        var currentUser = httpContext?.User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

        if (currentUser == null)
            return;
        var user = await ctx.Users.FirstOrDefaultAsync(u => u.Username == currentUser);
        if (user != null) return;

        var newUser = new User { Username = currentUser };
        ctx.Users.Add(newUser);
        await ctx.SaveChangesAsync();

        var log = new AuraPointsLog()
        {
            UserId = newUser.Id,
            Points = 1000,
            CreatedOn = DateTime.UtcNow.ToUnixTime()
        };
        ctx.AuraPointsLogs.Add(log);
        await ctx.SaveChangesAsync();
    }
}
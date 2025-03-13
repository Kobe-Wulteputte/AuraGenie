using System.Text.Json.Serialization;
using AuraGenie.Api.Extensions;
using AuraGenie.Api.Hubs;
using AuraGenie.Data.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);
try
{
    builder.Services.AddOpenApi();
    builder.Services.AddBusiness();
    builder.Services.AddData(builder.Configuration);
    builder
        .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAD"));
    builder.Services.AddAuthorization();
    var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();
    builder.Services.ConfigureHttpJsonOptions(options => { options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; });
    builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: "Api", policy =>
            {
                policy
                    .WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition");
            });
        }
    );
    builder.Services.AddOpenApiDocument(options => { options.Title = "API"; });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseOpenApi(options => { options.Path = "/swagger/v1/swagger.json"; });
    }

    var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<AuraContext>();
    Console.WriteLine(ctx.Rooms.ToList());

    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseAuthorization();
    app.UseAuthentication();
    app.MapHub<MessageHub>("/api/message/socket");
    app.UseHttpsRedirection();
    app.UseCors("Api");
    app.AddEndpoints();
    app.Run();
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}
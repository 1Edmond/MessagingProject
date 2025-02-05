using MessagingWebService.Controllers;
using MessagingWebService.Interfaces;
using MessagingWebService.Services;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Console.OutputEncoding = Encoding.UTF8;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    //.MinimumLevel.Information()

    //.WriteTo.Console()
    //.WriteTo.File("/logs/log-.txt", rollingInterval: RollingInterval.Day, encoding: Encoding.UTF8)
    .CreateLogger();

builder.Services.AddControllers();

builder.Services.AddTransient<IAppMessageRepository, AppMessageRepository>();

builder.Services.AddSingleton<AppMessageWebSocketService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog();

var app = builder.Build();


app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
});

app.UseCors(builder => builder
     .AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}




app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

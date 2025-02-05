using MessagingWebService.Controllers;
using MessagingWebService.Interfaces;
using MessagingWebService.Services;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.OpenApi.Models;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebSockets(options =>
{
    
    options.KeepAliveInterval = TimeSpan.FromSeconds(25);
});


builder.Services.AddControllers();

builder.Services.AddTransient<IAppMessageRepository, AppMessageRepository>();

builder.Services.AddSingleton<AppMessageWebSocketService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();






var app = builder.Build();

app.UseCors(builder => builder
     .AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(25)
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

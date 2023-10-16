using ChatService.RabbitMQServices;
using JwtTokenManager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors();
builder.Services.AddJwtAuthExtension();

try
{
    ConnectionFactory connectionFactory = new ConnectionFactory
    {
        Uri = new Uri("amqps://gtyepqer:MFoGZBk-zqtRAf8fZoKPYIdBIcQTOp8T@fly.rmq.cloudamqp.com/gtyepqer")
    };
    var connection = connectionFactory.CreateConnection();
    var channel = connection.CreateModel();

    // accept only one unack-ed message at a time
    // uint prefetchSize, ushort prefetchCount, bool global
    channel.BasicQos(0, 1, false);
    RabbitmqConsumer messageReceiver = new RabbitmqConsumer(channel);
    channel.QueueDeclare("chat", exclusive: false);
    channel.BasicConsume("chat", false, messageReceiver);
}
catch (BrokerUnreachableException ex)
{
    
    Console.WriteLine(ex.ToString());
}

//builder.Services.AddCors(p => p.AddPolicy("corspolicy", build =>
//{
//    build.SetIsOriginAllowed(isOriginAllowed: _ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
//}));
var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("Content-Disposition")
);
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

using InfoServerGRPC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseGrpcWeb();
app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<InfoServerGRPCImpl>().EnableGrpcWeb();
    endpoints.MapControllers();
});

app.Run();

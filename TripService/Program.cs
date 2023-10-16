//using TripService.RabbitMQServices;
using JwtTokenManager;
using TripService;
using TripService.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
    
builder.Services.AddGraphQLServer().AddQueryType<Query>();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddCors();
builder.Services.AddJwtAuthExtension();
//builder.Services.AddTransient<IMessageProducer, RabbitmqProducer>();
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
};
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGraphQL("/trip/graphql");
app.Run();

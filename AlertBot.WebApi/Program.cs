using AlertBot.WebApi.ServiceExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices();
builder.Services.AddBackgroundServices();

var app = builder.Build();

await app.RunAsync();
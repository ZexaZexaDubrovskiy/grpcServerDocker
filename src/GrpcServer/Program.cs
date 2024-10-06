using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Настройка Kestrel для поддержки HTTP/2 без шифрования и прослушивания на всех IP-адресах
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddGrpc();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var app = builder.Build();

// Настройка HTTP-запросов и конечных точек.
app.MapGrpcService<GrpcServer.Implementation.DataServiceImpl>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();

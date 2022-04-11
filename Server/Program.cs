using System.Net;
using Contracts.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Lite;
using ProtoBuf.Grpc.Lite.Connections;
using ProtoBuf.Grpc.Server;
using Server.Services;

namespace Server;

public static class Program
{
    public static async Task Main() =>
        await WebHost.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.Configure<HostFilteringOptions>(options => options.AllowedHosts = new[] { "*" });

                services.AddCodeFirstGrpc();
                services.AddSingleton<ITestService, TestService>();
                services.AddSingleton<LiteServer>();
                services.AddSingleton<GrpcTcpHandler>();
                services.AddSingleton(provider =>
                {
                    var server = new LiteServer();
                    server.ServiceBinder.AddCodeFirst(provider.GetRequiredService<ITestService>());
                    
                    return server;
                });
            })
            .UseKestrel(options => options.Listen(new IPEndPoint(IPAddress.Loopback, 5000), o => o.UseConnectionHandler<GrpcTcpHandler>()))
            .Configure((_, app) =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGrpcService<TestService>();
                });
            })
            .Build()
            .RunAsync();
}

public sealed class GrpcTcpHandler : ConnectionHandler
{
    private readonly LiteServer _server;
    public GrpcTcpHandler(LiteServer server) => _server = server;

    public override async Task OnConnectedAsync(ConnectionContext connection) => await _server.ListenAsync(connection.Transport.AsFrames());
}
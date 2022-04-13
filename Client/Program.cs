using System.Globalization;
using System.Net;
using System.Reactive.Linq;
using Contracts.Services;
using ProtoBuf.Grpc.Client;
using ProtoBuf.Grpc.Lite;

namespace Client;

public static class Program
{
    public static async Task Main()
    {
        await using var channel = await ConnectionFactory
            .ConnectSocket(new IPEndPoint(IPAddress.Loopback, 5000))
            .AsStream()
            .AsFrames()
            .CreateChannelAsync(TimeSpan.FromSeconds(5));

        var service = channel.CreateGrpcService<ITestService>();

        Console.WriteLine(service.Echo("Creating subscription"));
        
        var requests = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Select(_ => DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));

        requests.Subscribe(request => Console.WriteLine($"[Client]: {request}"));
                    
        service.Subscribe(requests)
            .Subscribe(response => Console.WriteLine($"[Server]: {response}"));

        Console.WriteLine(service.Echo("Subscription created"));
        Console.ReadLine();
    }
}
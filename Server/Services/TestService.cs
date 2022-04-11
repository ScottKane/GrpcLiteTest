using System.Globalization;
using System.Reactive.Linq;
using Contracts.Services;

namespace Server.Services;

public class TestService : ITestService
{
    public string Echo(string message)
    {
        message = $"[Server][{DateTime.UtcNow}]: {message}";
        Console.WriteLine(message);
        return message;
    }
    
    public IAsyncEnumerable<string> Subscribe(IAsyncEnumerable<string> requests)
    {
        requests
            .ToObservable()
            .Subscribe(request => Console.WriteLine($"[Client]: {request}"));

        var responses = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Select(_ => DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));

        responses.Subscribe(response => Console.WriteLine($"[Server]: {response}"));
        
        return responses.ToAsyncEnumerable();
    }
}
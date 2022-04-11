using System.ServiceModel;

namespace Contracts.Services;

[ServiceContract]
public interface ITestService
{
    [OperationContract] string Echo(string message);
    [OperationContract] IAsyncEnumerable<string> Subscribe(IAsyncEnumerable<string> requests);
}
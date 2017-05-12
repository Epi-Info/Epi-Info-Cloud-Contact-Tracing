using Epi.Cloud.ServiceBus;

namespace Epi.Cloud.DataConsistencyServices.Proxy
{
    public interface IResponseInfoServiceBus
    {
        MessagePayload GetResponseInfoMessageFromServiceBus();
    }
}

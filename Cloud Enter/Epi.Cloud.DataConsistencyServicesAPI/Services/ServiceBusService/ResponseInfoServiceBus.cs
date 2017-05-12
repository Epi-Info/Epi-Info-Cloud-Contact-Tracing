using Epi.Cloud.DataConsistencyServices.Proxy;
using Epi.Cloud.ServiceBus;


namespace Epi.Cloud.DataConsistencyServices.Services.ServiceBusService
{
    public class ResponseInfoServiceBus : IResponseInfoServiceBus
    {
        public ResponseInfoServiceBus()
        {

        }

        public MessagePayload GetResponseInfoMessageFromServiceBus()
        {
            ServiceBusCRUD crudServiceBus = new ServiceBusCRUD();
            var messagePayload = crudServiceBus.ReceiveMessages();
            return messagePayload;
        }
    }
}
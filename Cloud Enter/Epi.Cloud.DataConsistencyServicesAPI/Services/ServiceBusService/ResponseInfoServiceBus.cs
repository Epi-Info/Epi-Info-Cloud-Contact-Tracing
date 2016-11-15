using Epi.Cloud.DataConsistencyServices.Proxy;
using Epi.Cloud.ServiceBus;


namespace Epi.Cloud.DataConsistencyServices.Services.ServiceBusService
{
    public class ResponseInfoServiceBus : IResponseInfoServiceBus
    {
        public ResponseInfoServiceBus()
        {

        }

        public string GetResponseInfoMessageFromServiceBus()
        {
            ServiceBusCRUD crudServiceBus = new ServiceBusCRUD();
            var responseInfoMessage = crudServiceBus.ReceiveMessages();
            return responseInfoMessage;
        }
    }
}
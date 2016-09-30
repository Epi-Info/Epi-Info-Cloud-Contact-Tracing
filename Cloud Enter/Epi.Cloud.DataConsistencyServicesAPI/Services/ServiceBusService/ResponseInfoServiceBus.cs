using Epi.Cloud.DataConsistencyServicesAPI.Proxy;
using Epi.Cloud.ServiceBus;


namespace Epi.Cloud.DataConsistencyServicesAPI.Services.ServiceBusService
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
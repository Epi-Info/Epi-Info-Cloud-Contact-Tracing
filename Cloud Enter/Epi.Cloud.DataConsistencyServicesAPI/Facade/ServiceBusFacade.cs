


using Epi.Cloud.ServiceBus;

namespace Epi.Cloud.DataConsistencyServicesAPI.Facade
{
    public class ServiceBusFacade
    {
        bool ReadFormdetailsFromServiceBus()
        {
            CURDServiceBus crudServiceBus = new CURDServiceBus();
            crudServiceBus.ReceiveMessages();

            return true;
        }
    }
}
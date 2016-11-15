using Epi.Cloud.ServiceBus;

namespace Epi.Cloud.DataConsistencyServices.Facade
{
    public class ServiceBusFacade
    {
        bool ReadFormDetailsFromServiceBus()
        {
            ServiceBusCRUD serviceBusCRUD = new ServiceBusCRUD();
            serviceBusCRUD.ReceiveMessages();

            return true;
        }
    }
}
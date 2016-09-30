using Epi.Cloud.DataConsistencyServicesAPI.Proxy;
using Epi.Cloud.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Epi.Cloud.DataConsistencyServicesAPI.Services.ServiceBusService
{
    public class FormInfoServiceBus : IFormInfoServiceBus
    {
        public FormInfoServiceBus()
        {

        }

        public string GetFormInfoFromServiceBus()
        {
            CURDServiceBus crudServiceBus = new CURDServiceBus();
            var formInfodetails=crudServiceBus.ReceiveMessages();
            return formInfodetails;
        }
    }
}
using System.Collections.Generic;

namespace Epi.Cloud.DataConsistencyServicesAPI.Proxy
{
    public interface IResponseInfoProxyService
    {
        string GetResponseInfoData(string responseId);
    }
}

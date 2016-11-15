using System.Collections.Generic;

namespace Epi.Cloud.DataConsistencyServices.Proxy
{
    public interface IResponseInfoProxyService
    {
        string GetResponseInfoData(string responseId);
    }
}

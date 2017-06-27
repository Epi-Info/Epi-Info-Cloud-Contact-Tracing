using System.Collections.Generic;
using Epi.Common.Core.Interfaces;

namespace Epi.Cloud.DataConsistencyServices.Proxy
{
    public interface IResponseInfoProxyService
    {
        string GetResponseInfoData(IResponseContext responseContext);
    }
}

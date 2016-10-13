using System.Collections.Generic;
using static Epi.Cloud.DataConsistencyServicesAPI.DataTypes.Constants;

namespace Epi.Cloud.DataConsistencyServicesAPI.DataTypes
{
    public class CDTResponse
    {
        public ResponseType Type { get; set; }
        public IDictionary<string, string> Messages { get; set; }
    }
}

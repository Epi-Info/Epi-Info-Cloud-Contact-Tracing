using System.Collections.Generic;

namespace Epi.Cloud.DataConsistencyServices.DataTypes
{
    public class CDTResponse
    {
        public Constants.ResponseType Type { get; set; }
        public IDictionary<string, string> Messages { get; set; }
    }
}

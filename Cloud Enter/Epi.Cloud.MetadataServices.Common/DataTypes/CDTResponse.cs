using System.Collections.Generic;
using static Epi.Cloud.MetadataServices.Common.DataTypes.Constants;

namespace Epi.Cloud.MetadataServices.Common.DataTypes
{
    public class CDTResponse
    {
        public ResponseType Type { get; set; }

        public IDictionary<string, string> Messages { get; set; }
    }
}

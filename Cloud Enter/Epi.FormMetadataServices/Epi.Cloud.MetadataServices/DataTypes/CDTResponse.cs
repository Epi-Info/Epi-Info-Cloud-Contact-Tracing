using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Epi.Cloud.MetadataServices.DataTypes.Constants;

namespace Epi.Cloud.MetadataServices.DataTypes
{
    public class CDTResponse
    {
        public ResponseType Type { get; set; }
        public IDictionary<string, string> Messages { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Cloud.MetadataServices.DataTypes
{
    public class CDTUser:CDTBase
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

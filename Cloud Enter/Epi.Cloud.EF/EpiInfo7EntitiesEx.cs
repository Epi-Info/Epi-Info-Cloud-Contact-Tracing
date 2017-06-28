using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Epi.Cloud.Common.Configuration;

namespace Epi.Data.EF
{
    public partial class EPIInfo7Entities
    {
        private static string GetConnectionString(string EntitiesName, bool ConnectionStringEncrypt)
        {
            return ConfigurationHelper.GetConnectionString(EntitiesName, ConnectionStringEncrypt);
        }
        public EPIInfo7Entities(string connectionStringName)
            : base(GetConnectionString("EPIInfo7Entities", true))
        {
        }
    }
}

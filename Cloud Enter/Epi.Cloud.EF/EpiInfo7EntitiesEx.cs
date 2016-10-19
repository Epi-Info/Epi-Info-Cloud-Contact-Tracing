using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Data.EF
{
    public partial class EPIInfo7Entities
    {
        public EPIInfo7Entities(string connectionStringName)
            : base("name="+ connectionStringName)
        {
        }
    }
}

using Epi.Cloud.Common.Constants;

namespace Epi.Data.EF
{
    public partial class EPIInfo7Entities
    {
        public EPIInfo7Entities(string connectionStringName)
            : base(ConnectionStrings.GetConnectionString(ConnectionStrings.Key.EPIInfo7Entities))
        {
        }
    }
}

using System.Web;
using System.Web.Mvc;

namespace Epi.Cloud.DataConsistencyServices
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}

using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Epi.Cloud.DataConsistencyServices.Startup))]

namespace Epi.Cloud.DataConsistencyServices
{
	public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

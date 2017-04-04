using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Cloud.WebJobs.Interface
{
    public interface IEpiCloudWebJob
    {
        void StartWebJob(string WebJobName);
    }
}

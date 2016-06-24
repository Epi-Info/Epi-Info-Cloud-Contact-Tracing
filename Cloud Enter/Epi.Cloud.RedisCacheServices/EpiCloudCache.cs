using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Cloud.CacheServices
{
    public partial class EpiCloudCache : RedisCache, IEpiCloudCache
    {
        public void ClearAllCache()
        {
            ClearAllMetadataFromCache();
            ClearAllSurveyInfoBoMetadataFromCache();
            DeleteAllKeys(MetadataPrefix);
            ClearAllSurveyIdMap();
        }
    }
}

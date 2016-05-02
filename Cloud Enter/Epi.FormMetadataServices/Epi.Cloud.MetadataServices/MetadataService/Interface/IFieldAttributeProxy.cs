using Epi.Cloud.MetadataServices.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Cloud.MetadataServices.ProxiesService.Interface
{  
        public interface IFieldAttributeProxy
        { 
            Task<List<MetadataFieldAttributes>> GetProjectMetadataAsync(string PageId);//Read project
       } 
}

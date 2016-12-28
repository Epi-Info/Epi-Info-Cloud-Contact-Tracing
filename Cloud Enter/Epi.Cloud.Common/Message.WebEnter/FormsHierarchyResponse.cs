using System.Runtime.Serialization;
using System.Collections.Generic;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.MessageBase;

namespace Epi.Cloud.Common.Message
{
    public class FormsHierarchyResponse : ResponseBase
    {
        public FormsHierarchyResponse()
        {
            this.FormsHierarchy = new List<FormsHierarchyDTO>();
        }

        public List<FormsHierarchyDTO> FormsHierarchy;
    }
}

﻿using System.Runtime.Serialization;
using System.Collections.Generic;
using Epi.Web.Enter.Common.DTO;

namespace Epi.Web.Enter.Common.Message
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class FormsHierarchyResponse : Epi.Web.Enter.Common.MessageBase.ResponseBase
    {
        public FormsHierarchyResponse()
        {
            this.FormsHierarchy = new List<FormsHierarchyDTO>();
        }

        [DataMember]
        public List<FormsHierarchyDTO> FormsHierarchy;
    }
}

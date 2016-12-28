using System.Runtime.Serialization;
using System.Collections.Generic;
using Epi.Cloud.Common.DTO;
using Epi.Web.Common;

namespace Epi.Cloud.Common.Message
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class FormSettingRequest //: Epi.Cloud.Common.MessageBase.RequestBase
    {
        public FormSettingRequest()
        {
            this.FormSetting = new List<FormSettingDTO>();
            this.FormInfo = new FormInfoDTO();
        }
        public string ProjectId
        {
            get { return FormInfo.ProjectId; }
            set { FormInfo.ProjectId = value; }
        }

        [DataMember]
        public List<FormSettingDTO> FormSetting { get; set; }
        [DataMember]
        public FormInfoDTO FormInfo { get; set; }
        [DataMember]
        public bool GetMetadata { get; set; }
        [DataMember]
        public int CurrentOrgId { get; set; }

    }
}

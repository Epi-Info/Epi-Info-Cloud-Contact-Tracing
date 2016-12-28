using System.Runtime.Serialization;
using Epi.Cloud.Common.DTO;

namespace Epi.Web.Enter.Common.Message
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class FormSettingResponse : Epi.Web.Enter.Common.MessageBase.RequestBase
    {

        public FormSettingResponse()
        {
            this.FormSetting = new FormSettingDTO();
            this.FormInfo = new FormInfoDTO();
        }
        [DataMember]
        public FormSettingDTO FormSetting;

        [DataMember]
        public FormInfoDTO FormInfo;
    }
}

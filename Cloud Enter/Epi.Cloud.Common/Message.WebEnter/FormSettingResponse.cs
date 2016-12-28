using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.MessageBase;

namespace Epi.Cloud.Common.Message
{
    public class FormSettingResponse : RequestBase
    {

        public FormSettingResponse()
        {
            this.FormSetting = new FormSettingDTO();
            this.FormInfo = new FormInfoDTO();
        }

        public FormSettingDTO FormSetting;

        public FormInfoDTO FormInfo;
    }
}

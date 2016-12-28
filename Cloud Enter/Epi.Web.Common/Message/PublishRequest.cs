using Epi.Cloud.Common.DTO;
using System.Runtime.Serialization;

namespace Epi.Web.Enter.Common.Message
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class PublishRequest : Epi.Web.Enter.Common.MessageBase.RequestBase
    {

        public PublishRequest()
        {
            this.SurveyInfo = new SurveyInfoDTO();
        }

        /// <summary>
        /// SurveyInfo object.
        /// </summary>
        [DataMember]
        public SurveyInfoDTO SurveyInfo;
    }
}

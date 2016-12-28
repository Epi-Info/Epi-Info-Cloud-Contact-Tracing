using Epi.Cloud.Common.DTO;

namespace Epi.Cloud.Common.Message
{
    /// <summary>
    /// Represents a Survey Response  request message from client.
    /// </summary>
    public class PreFilledAnswerRequest
    {

        public PreFilledAnswerRequest()
        {
            this.AnswerInfo = new PreFilledAnswerDTO();
        }

        /// <summary>
        /// AnswerInfo object.
        /// </summary>
        public PreFilledAnswerDTO AnswerInfo;
    }
}

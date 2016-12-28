using Epi.Cloud.Common.MessageBase;
using Epi.Cloud.Common.DTO;

namespace Epi.Cloud.Common.Message
{
    public class UserAuthenticationRequest : RequestBase
    {
        public UserAuthenticationRequest() { }
        //This code stays but will not be used in WebEnter
        //Starts
        public string SurveyResponseId;

        public string PassCode;

        public UserDTO User;
    }
}

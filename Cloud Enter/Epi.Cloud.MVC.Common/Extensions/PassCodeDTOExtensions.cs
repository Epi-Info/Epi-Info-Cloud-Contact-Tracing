using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Message;

namespace Epi.Cloud.MVC.Extensions
{
    public static class PassCodeDTOExtensions
    {
        public static UserAuthenticationRequest ToUserAuthenticationObj(this PassCodeDTO passCodeDTO)
        {
            return new UserAuthenticationRequest
            {
                SurveyResponseId = passCodeDTO.ResponseId,
                PassCode = passCodeDTO.PassCode
            };
        }
    }
}

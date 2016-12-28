using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Message;

namespace Epi.Cloud.Common.Extensions
{
    public static class UserAuthenticationRequestExtensions
    {
        public static UserAuthenticationRequestBO ToPassCodeBO(this UserAuthenticationRequest UserAuthenticationObj)
        {
            return new UserAuthenticationRequestBO
            {
                ResponseId = UserAuthenticationObj.SurveyResponseId,
                PassCode = UserAuthenticationObj.PassCode
            };
        }
    }
}

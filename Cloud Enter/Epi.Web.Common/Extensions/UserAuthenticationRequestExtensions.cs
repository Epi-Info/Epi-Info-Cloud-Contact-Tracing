using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.Message;

namespace Epi.Web.Enter.Common.Extensions
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

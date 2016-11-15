using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.Message;

namespace Epi.Web.Enter.Common.Extensions
{
    public static class UserAuthenticationResponseBOExtensions
    {
        public static UserAuthenticationResponse ToUserAuthenticationResponse(this UserAuthenticationResponseBO AuthenticationRequestBO)
        {
            return new UserAuthenticationResponse
            {
                PassCode = AuthenticationRequestBO.PassCode,
            };
        }
    }
}

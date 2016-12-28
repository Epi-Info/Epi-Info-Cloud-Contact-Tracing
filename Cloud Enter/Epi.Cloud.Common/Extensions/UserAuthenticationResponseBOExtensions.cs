using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Message;

namespace Epi.Cloud.Common.Extensions
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

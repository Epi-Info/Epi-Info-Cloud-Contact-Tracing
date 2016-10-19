using Epi.Cloud.Common.DTO;
using Epi.Web.Enter.Common.Message;

namespace Epi.Web.MVC.Facade
{
    public interface ISecurityFacade
    {
        UserAuthenticationResponse ValidateUser(string userName, string password);
        void UpdatePassCode(string responseId, string passcode);
        UserAuthenticationResponse GetAuthenticationResponse(string responseId);
        UserAuthenticationResponse GetUserInfo(int UserId);
        bool UpdateUser(Enter.Common.DTO.UserDTO User);
    }
}

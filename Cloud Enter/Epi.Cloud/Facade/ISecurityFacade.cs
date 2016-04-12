using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.Enter.Common.Message;

namespace Epi.Web.MVC.Facade
{
    public interface ISecurityFacade
    {
        UserAuthenticationResponse ValidateUser(string userName, string password);
        void UpdatePassCode(string responseId, string passcode);
        UserAuthenticationResponse GetAuthenticationResponse(string responseId);
        UserAuthenticationResponse GetUserInfo(int UserId);
        bool UpdateUser(UserDTO User);
    }
}

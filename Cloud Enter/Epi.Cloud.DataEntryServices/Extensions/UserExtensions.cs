using System;
using Epi.Web.Enter.Common.BusinessObject;

namespace Epi.Cloud.DataEntryServices.Extensions
{
    public static class UserExtensions
    {
        public static UserBO ToUserBO(this Epi.Web.EF.User user, int Role = 0)
        {
            UserBO userBO = new UserBO();
            userBO.UserId = user.UserID;
            userBO.UserName = user.UserName;
            userBO.EmailAddress = user.EmailAddress;
            userBO.FirstName = user.FirstName;
            userBO.LastName = user.LastName;
            userBO.PhoneNumber = user.PhoneNumber;
            userBO.ResetPassword = user.ResetPassword;
            userBO.Role = Role;
            if (!string.IsNullOrEmpty(user.UGuid.ToString()))
            {
                userBO.UGuid = Guid.Parse(user.UGuid.ToString());
            }
            return userBO;
        }
    }
}

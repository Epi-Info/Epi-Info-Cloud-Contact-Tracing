using Epi.Web.Enter.Common.BusinessObject;

namespace Epi.Cloud.DataEntryServices.Extensions
{
    public static class UserBOExtensions
    {
        public static Epi.Web.EF.User ToUserEntity(this UserBO userBO)
        {
            var user = new Epi.Web.EF.User();
            user.EmailAddress = userBO.EmailAddress;
            user.UserName = userBO.EmailAddress;
            user.LastName = userBO.LastName;
            user.FirstName = userBO.FirstName;
            user.PhoneNumber = userBO.PhoneNumber;
            user.ResetPassword = userBO.ResetPassword;
            user.PasswordHash = userBO.PasswordHash;
            user.UGuid = userBO.UGuid;
            return user;
        }
    }
}

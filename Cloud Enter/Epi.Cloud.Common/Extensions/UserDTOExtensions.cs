using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;

namespace Epi.Cloud.Common.Extensions
{
    public static class UserDTOExtensions
    {
        public static UserBO ToUserBO(this UserDTO User)
        {
            return new UserBO
            {
                UserId = User.UserId,
                UserName = User.UserName,
                FirstName = User.FirstName,
                LastName = User.LastName,
                EmailAddress = User.EmailAddress,
                PhoneNumber = User.PhoneNumber,
                PasswordHash = User.PasswordHash,
                ResetPassword = User.ResetPassword,
                Role = User.Role,
                UserHighestRole = User.UserHighestRole,
                IsActive = User.IsActive,
                Operation = (Constant.OperationMode)User.Operation,
                UGuid = User.UGuid
            };
        }
    }
}

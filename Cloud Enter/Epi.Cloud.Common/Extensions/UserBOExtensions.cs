using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;
using System.Linq;
using System.Collections.Generic;

namespace Epi.Cloud.Common.Extensions
{
    public static class UserBOExtensions
    {
        public static UserDTO ToUserDTO(this UserBO result)
        {
            return new UserDTO()
            {
                UserId = result.UserId,
                UserName = result.UserName,
                FirstName = result.FirstName,
                LastName = result.LastName,
                PasswordHash = result.PasswordHash,
                PhoneNumber = result.PhoneNumber,
                ResetPassword = result.ResetPassword,
                Role = result.Role,
                UserHighestRole = result.UserHighestRole,
                Operation = Constant.OperationMode.NoChange,
                EmailAddress = result.EmailAddress,
                IsActive = result.IsActive,
                UGuid = result.UGuid
            };
        }

        public static List<UserDTO> ToUserDTOList(this List<UserBO> userBOList)
        {
            return userBOList.Select(u => u.ToUserDTO()).ToList();
        }
    }
}

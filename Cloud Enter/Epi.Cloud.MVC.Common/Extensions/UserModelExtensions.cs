using Epi.Cloud.Common.DTO;
using Epi.Web.MVC.Models;

namespace Epi.Cloud.MVC.Extensions
{
    public static class UserModelExtensions
    {
        public  static UserDTO ToUserDTO(this UserModel UserModel)
        {
            UserDTO UserDTO = new UserDTO();

            UserDTO.EmailAddress = UserModel.Email;
            UserDTO.UserId = UserModel.UserId;
            UserDTO.FirstName = UserModel.FirstName;
            UserDTO.LastName = UserModel.LastName;
            UserDTO.Role = int.Parse(UserModel.Role);
            UserDTO.IsActive = UserModel.IsActive;
            if (!string.IsNullOrEmpty(UserModel.PhoneNumber))
            {
                UserDTO.PhoneNumber = UserModel.PhoneNumber;
            }
            else
            {
                UserDTO.PhoneNumber = "123456789";
            }
            return UserDTO;
        }
    }
}

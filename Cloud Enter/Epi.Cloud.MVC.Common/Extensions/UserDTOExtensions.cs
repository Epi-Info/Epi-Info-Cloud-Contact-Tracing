using System.Collections.Generic;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.MVC.Models;

namespace Epi.Cloud.MVC.Extensions
{
    public static class UserDTOExtensions
    {
        public static List<UserModel> ToUserModelList(this List<UserDTO> UserList)
        {
            List<UserModel> UserModel = new List<UserModel>();
            foreach (var user in UserList)
            {
                UserModel.Add(user.ToUserModel());
            }
            return UserModel;
        }

        public static UserModel ToUserModel(this UserDTO user)
        {
            UserModel UserModel = new UserModel();
            UserModel.Email = user.EmailAddress;
            UserModel.FirstName = user.FirstName;
            UserModel.LastName = user.LastName;
            UserModel.Role = GetUserRole(user.Role);
            UserModel.IsActive = user.IsActive;
            UserModel.UserId = user.UserId;
            return UserModel;
        }

        public static UserModel ToUserModelR(this UserDTO user)
        {
            UserModel UserModel = new UserModel();
            UserModel.Email = user.EmailAddress;
            UserModel.FirstName = user.FirstName;
            UserModel.LastName = user.LastName;
            UserModel.Role = user.Role.ToString();
            UserModel.IsActive = user.IsActive;
            UserModel.UserId = user.UserId;
            return UserModel;
        }

        private static string GetUserRole(int p)
        {
            string Role = "";
            switch (p)
            {
                case 1:
                    Role = "Analyst";
                    break;

                case 2:
                    Role = "Administrator";
                    break;

                case 3:
                    Role = "Super Administrator";
                    break;

                default:
                    break;
            }
            return Role;
        }
    }
}

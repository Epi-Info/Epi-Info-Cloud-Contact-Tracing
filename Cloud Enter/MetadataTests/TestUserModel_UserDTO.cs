using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.MVC.Models;
using Epi.Cloud.Common.DTO;
using System.Linq;

namespace MetadataTests
{
    [TestClass]
    public class TestUserModel_UserDTO
    {
        private const int UserModelPropertyCount = 9;
        private const int UserDTOPropertyCount = 13;
        [TestMethod]
        public void TestUserModelDTO()
        {
            UserModel objUserModel = new UserModel();
            UserDTO objUserDTO = new UserDTO();
            int numberOfPublicPropertiesofUserModel = objUserModel.GetType().GetProperties().Count();
            int numberOfPublicPropertiesofUserDTO = objUserDTO.GetType().GetProperties().Count();
            Assert.AreEqual(numberOfPublicPropertiesofUserModel, UserModelPropertyCount, "UserModel properties has been changed");
            Assert.AreEqual(numberOfPublicPropertiesofUserDTO, UserDTOPropertyCount, "UserDTO properties has been changed");

        }
    }
}

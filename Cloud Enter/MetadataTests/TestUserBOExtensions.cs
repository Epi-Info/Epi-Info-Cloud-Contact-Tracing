using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Extensions;

namespace MetadataTests
{
    [TestClass]
    public class TestUserBOExtensions
    {
        [TestMethod]
        public void TestMethod1()
        {
            var userBO = new UserBO()
            {
                UserId = 1014,
                UserName = "Ananth_Raja@sra.com",
                FirstName = "Ananth",
                LastName = "Raja",
                PasswordHash = "null",
                EmailAddress = "Ananth_Raja@sra.com",
                PhoneNumber = "1234567891",
                Role = 0,
                UGuid = new Guid("6e24fccd-d406-4148-9b0c-e1cf08e4b744"),
                UserHighestRole = 0,
                IsActive = false

            };
            var user = UserBOExtensions.ToUserDTO(userBO);
            Assert.AreEqual(user.FirstName, userBO.FirstName);
            Assert.AreEqual(user.PhoneNumber, userBO.PhoneNumber);
            Assert.AreEqual(user.EmailAddress, userBO.EmailAddress);
            Assert.IsFalse(user.PhoneNumber.Length > 10);
            Assert.IsFalse(userBO.PhoneNumber.Length > 10);
           

        }
    }
}

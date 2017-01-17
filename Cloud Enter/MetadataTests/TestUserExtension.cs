using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Web.EF;
using Epi.Cloud.Common.Extensions;

namespace MetadataTests
{
    [TestClass]
    public class TestUserExtension
    {
        [TestMethod]
        public void TestMethod1()
        {
            var user = new User()
            {
                UserID = 1014,
                UserName = "Ananth_Raja@sra.com",
                FirstName = "Ananth",
                LastName = "Raja",
                PasswordHash = "null",
                EmailAddress = "Ananth_Raja@sra.com",
                PhoneNumber = "1234567891",
                UGuid = new Guid("6e24fccd-d406-4148-9b0c-e1cf08e4b744")
            
            };
            var Userext = Epi.Cloud.DataEntryServices.Extensions.UserExtensions.ToUserBO(user, role:0);
            Assert.AreEqual(user.EmailAddress, Userext.EmailAddress);
            Assert.AreEqual(user.UserID, Userext.UserId);
            Assert.AreEqual(user.UserName, Userext.UserName);


        }
    }
}

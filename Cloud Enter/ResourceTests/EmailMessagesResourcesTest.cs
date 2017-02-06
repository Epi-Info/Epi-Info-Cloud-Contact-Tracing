using Epi.Cloud.Resources;
using Epi.Cloud.Resources.Constants;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ResourceTests
{
    [TestClass]
    public class EmailMessages
    {
        const string ResourcesAssemblyName = "Epi.Cloud.Resources";
        const string EmailMessagesResourcesNamespace = "Epi.Cloud.Resources.EmailMessages";

        static System.Reflection.Assembly _resourcesAssembly;
        static System.Resources.ResourceManager _resourceManager;

        [TestInitialize]
        public void Initialize()
        {
            // Gets a reference to a different assembly.
            _resourcesAssembly = System.Reflection.Assembly.Load(ResourcesAssemblyName);
            _resourceManager = new
               System.Resources.ResourceManager(EmailMessagesResourcesNamespace,
               _resourcesAssembly);
        }

        [TestMethod]
        public void TestGetResourceString()
        {
            // Retrieves a resource message
            System.String testString;
            testString = _resourceManager.GetString("AMessageForUnitTests");
            Assert.AreEqual("This is a test", testString);
        }

        [TestMethod]
        public void TestGetResourceString2()
        {
            // Retrieves a resource message
            System.String testString;
            testString = ResourceProvider.GetResourceString(ResourceNamespaces.EmailMessages, "AMessageForUnitTests");
            Assert.AreEqual("This is a test", testString);
        }
    }
}

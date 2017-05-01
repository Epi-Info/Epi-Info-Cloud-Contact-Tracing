using Epi.Common.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DocumentDBTests
{
    [TestClass]
    public class WildCardExtensionTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var value = "bill";
            var pattern = "b*";
            var isMatched = value.WildcardCompare(pattern);
            Assert.IsTrue(isMatched);
        }
    }
}

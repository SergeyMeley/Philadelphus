using Philadelphus.Business.Helpers;

namespace Philadelphus.Tests.Business
{
    [TestClass]
    public class NamingHelperTests
    {
        [TestMethod]
        [DataRow("Новый корень 3", "Новый корень", new string[] { "Новый корень 1", "Новый корень 2" })]
        [DataRow("Новый корень 8", "Новый корень", new string[] { "Новый корень 1", "Новый корень 2", "Новый корень 3", "Новый корень 4", "Новый корень 5", "Новый корень 6", "Новый корень 7" })]
        [DataRow("Новый корень 2", "Новый корень", new string[] { "Новый корень 1", "Новый корень 3" })]
        [DataRow("Новый корень 3", "Новый корень  ", new string[] { "Новый корень 1", "Новый корень 2" })]
        [DataRow("Новый корень 1", "Новый корень", new string[] { "Новый корень1", "Новый корень2" })]
        [DataRow("Новый корень 1", "Новый корень", new string[] { "sdfsfsdf", "dsfsfsdf" })]
        [DataRow("Новый корень 3", "Новый корень", new string[] { "Новый корень 2", "Новый корень 1" })]
        [DataRow(" 3", "", new string[] { " 1", " 2" })]
        public void TestMethod1(string resultName, string fixPart, string[] existNames)
        {
            string factResult = NamingHelper.GetNewName(existNames, fixPart);
            Assert.IsTrue(resultName == factResult);
        }
    }
}
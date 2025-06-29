using Philadelphus.WpfApplication.Models.Settings;

namespace Philadelphus.WpfApplicationTests.SettingsTests
{
    [TestClass]
    public class RepositoryListPathTests
    {

        [TestMethod]
        public void RepositoryListDefaultPathTests()
        {
            Assert.IsTrue(string.IsNullOrEmpty(ApplicationSettings.DefaultRepositoryPath) == false);
        }

        [TestMethod]
        public void RepositoryListExistPathTests()
        {
            string path = "test_path";
            ApplicationSettings.RepositoryListConfigPath = path;
            Assert.IsTrue(ApplicationSettings.RepositoryListConfigPath == path);
        }

        [TestMethod]
        public void RepositoryListNullPathTests()
        {
            ApplicationSettings.RepositoryListConfigPath = null;
            Assert.IsTrue(ApplicationSettings.RepositoryListConfigPath == ApplicationSettings.DefaultRepositoryPath);
        }
    }
}
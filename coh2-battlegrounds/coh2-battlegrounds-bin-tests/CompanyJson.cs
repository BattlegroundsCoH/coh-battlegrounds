using System.IO;
using System.Threading;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Game.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests {
    
    [TestClass]
    public class CompanyJson {

        Company company;

        [TestInitialize]
        public void Setup() {
            BlueprintManager.CreateDatabase();
            BlueprintManager.LoadDatabaseWithMod("battlegrounds", "142b113740474c82a60b0a428bd553d5");
            company = CompanyTestBuilder.CreateSovietCompany();
        }

        [TestMethod]
        public void SaveTest1() {
            company.SaveToFile("testCompany1.json");
            Assert.IsTrue(File.Exists("testCompany1.json"));
        }

        [TestMethod]
        public void SaveTest2() {
            company.SaveToFile("testCompany2.json");
            Assert.IsTrue(File.Exists("testCompany2.json"));
            Company read = Company.ReadCompanyFromFile("testCompany2.json");
            Assert.IsNotNull(read);
        }

    }

}

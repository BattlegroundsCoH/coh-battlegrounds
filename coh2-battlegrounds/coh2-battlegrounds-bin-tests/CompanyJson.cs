using System.IO;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
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

        [TestMethod]
        public void CanSerialiseAndDeserialise() {
            var str = CompanySerializer.GetCompanyAsJson(company, true);
            File.WriteAllText("testCompanyJson.json", str);
            Assert.IsNotNull(str);
            Company read = CompanySerializer.GetCompanyFromJson(str);
            Assert.IsNotNull(read);
            Assert.AreEqual(read.Checksum, company.Checksum);
        }

    }

}

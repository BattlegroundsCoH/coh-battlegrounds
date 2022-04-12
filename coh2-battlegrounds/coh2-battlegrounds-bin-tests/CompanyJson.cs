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
            BlueprintManager.LoadDatabaseWithMod("mod_bg", "142b113740474c82a60b0a428bd553d5");
            company = CompanyTestBuilder.CreateSovietCompany();
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

        [TestMethod]
        public void CanSerialiseAndDeserialiseAdvancedCompany() {
            var company = CompanyTestBuilder.CreateAdvancedSovietCompany();
            var str = CompanySerializer.GetCompanyAsJson(company, true);
            File.WriteAllText("testCompanyAdvancedJson.json", str);
            Assert.IsNotNull(str);
            Company read = CompanySerializer.GetCompanyFromJson(str);
            Assert.IsNotNull(read);
            Assert.AreEqual(read.Checksum, company.Checksum);
        }

    }

}

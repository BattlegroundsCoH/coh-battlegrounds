using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Game.Database;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests {
    
    [TestClass]
    public class CompanyImportExportTest {

        Company sovietCompany;

        [TestInitialize]
        public void Setup() {
            BlueprintManager.CreateDatabase();
            BlueprintManager.LoadDatabaseWithMod("battlegrounds", "142b113740474c82a60b0a428bd553d5");
            sovietCompany = CompanyTestBuilder.CreateSovietCompany();
        }

        [TestMethod]
        public void ToTemplateSoviet() {
            var t = CompanyTemplate.FromCompany(sovietCompany);
            var back = CompanyTemplate.FromString(t.ToString());
            Assert.AreEqual(t.TemplateName, back.TemplateName);
            Assert.AreEqual(t.TemplateArmy, back.TemplateArmy);
            Assert.AreEqual(t.TemplateUnitCount, back.TemplateUnitCount);
            Assert.IsTrue(back.TemplateUnitCount > 0);
        }

    }

}

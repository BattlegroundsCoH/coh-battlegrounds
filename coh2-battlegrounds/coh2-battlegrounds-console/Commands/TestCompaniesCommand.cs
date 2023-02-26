using Battlegrounds.Game.DataCompany;

using System.IO;

namespace Battlegrounds.Developer.Commands;

class TestCompaniesCommand : Command {

    public static readonly Argument<bool> SOV = new Argument<bool>("-s", "Specifies the Soviet company should *NOT* be generated.", true);
    public static readonly Argument<bool> GER = new Argument<bool>("-g", "Specifies the German company should *NOT* be generated.", true);
    public static readonly Argument<bool> RAW = new Argument<bool>("-raw", "Specifies the output format should not be formatted.", true);

    public TestCompaniesCommand() : base("company-test", "Compiles a German and Soviet test company using the most up-to-date version.", SOV, GER, RAW) { }

    public override void Execute(CommandArgumentList argumentList) {

        Program.LoadBGAndProceed();

        if (argumentList.GetValue(SOV))
            File.WriteAllText("26th_Rifle_Division.json", CompanySerializer.GetCompanyAsJson(Generator.CompanyGenerator.CreateSovietCompany(Program.tuningMod), argumentList.GetValue(RAW)));

        if (argumentList.GetValue(GER))
            File.WriteAllText("69th_panzer.json", CompanySerializer.GetCompanyAsJson(Generator.CompanyGenerator.CreateGermanCompany(Program.tuningMod), argumentList.GetValue(RAW)));

    }

}

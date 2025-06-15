namespace Battlegrounds.Developer.Commands;

public class CampaignCompileCommand : Command {

    public static readonly Argument<string> PATH = new Argument<string>("-c", "Specifies the directory to compile", string.Empty);
    public static readonly Argument<string> OUT = new Argument<string>("-o", "Specifies compiled campaign output file.", "campaign.camp");

    public CampaignCompileCommand() : base("campaign", "Compiles a campaign directory..", PATH, OUT) { }

    public override void Execute(CommandArgumentList argumentList) {

        Program.LoadBGAndProceed();
        // CampaignCompiler.Output = argumentList.GetValue(PATH);
        CampaignCompiler.Compile(argumentList.GetValue(OUT));

    }

}

namespace Battlegrounds.Developer.Commands;

public class MapExtractCommand : Command {

    public static readonly Argument<string> PATH = new Argument<string>("-o", "Specifies output directory.", "archive_maps");

    public static readonly Argument<bool> TEST = new Argument<bool>("-t", "Specifies if the testmap should be read instead", false);

    public static readonly Argument<bool> COH3 = new Argument<bool>("-coh3", "Specifies the tool should extract CoH3 maps instead of CoH2 maps", false);

    public MapExtractCommand() : base("coh2-extract-maps", "Extracts all CoH2 maps.", PATH, TEST, COH3) { }

    public override void Execute(CommandArgumentList argumentList) {

        if (argumentList.GetValue(TEST)) {
            MapExtractor.ReadTestmap();
            return;
        }

        MapExtractor.Output = argumentList.GetValue(PATH);
        if (argumentList.GetValue(COH3)) {
            MapExtractor.ExtractCoH3("coh3_maps");
        } else {
            MapExtractor.Extract();
        }

    }

}

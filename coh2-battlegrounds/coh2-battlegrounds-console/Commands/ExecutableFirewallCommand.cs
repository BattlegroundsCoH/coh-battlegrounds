using System.IO;

namespace Battlegrounds.Developer.Commands;

public class ExecutableFirewallCommand : Command {

    public ExecutableFirewallCommand() : base("add-firewallexeption", "Adds FirewallExceptio to executable") { }

    public override void Execute(CommandArgumentList argumentList) {

        string workdir = Path.GetFullPath("..\\..\\..\\..\\");

        string oldHeader = "<Wix xmlns=\"http://schemas.microsoft.com/wix/2006/wi\">";
        string newHeader = "<Wix xmlns=\"http://schemas.microsoft.com/wix/2006/wi\" xmlns:fire=\"http://schemas.microsoft.com/wix/FirewallExtension\">";

        string oldComponent = "<File Id=\"filC3836236C73ECA8506FB24920C9B8D22\" KeyPath=\"yes\" Source=\"$(var.BasePath)\\coh2-battlegrounds.exe\" />";
        string newComponent = "<File Id=\"filC3836236C73ECA8506FB24920C9B8D22\" KeyPath=\"yes\" Source=\"$(var.BasePath)\\coh2-battlegrounds.exe\"><fire:FirewallException Id=\"ExeFirewall\" Name=\"!(loc.ProductNameFolder)\" Port=\"11000\" Protocol=\"tcp\" Profile=\"public\" Scope=\"any\"/></File>";

        string file = Path.Combine(workdir, "coh2-battlegrounds-installer\\ComponentsGen.wxs");

        StreamReader reader = new(file);
        string content = reader.ReadToEnd();
        reader.Close();

        content = content.Replace(oldHeader, newHeader);
        content = content.Replace(oldComponent, newComponent);

        StreamWriter writer = new(file);
        writer.Write(content);
        writer.Close();

    }

}

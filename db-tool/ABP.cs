using System.ComponentModel;
using System.Xml;

namespace CoH2XML2JSON {
    
    public class ABP : BP {

        public override string Name { get; }

        public override string ModGUID { get; }

        public override ulong PBGID { get; }

        public Cost Cost { get; }

        public UI Display { get; }

        public string Army { get; init; }

        public string Activation { get; }

        [DefaultValue(null)]
        public Requirement[] Requirements { get; }

        public ABP(XmlDocument xmlDocument, string guid, string name) {

            // Set the name
            this.Name = name;

            // Set mod GUID
            this.ModGUID = string.IsNullOrEmpty(guid) ? null : guid;

            // Load pbgid
            this.PBGID = ulong.Parse(xmlDocument["instance"]["uniqueid"].GetAttribute("value"));

            // Load display
            this.Display = new(xmlDocument.SelectSingleNode(@"//group[@name='ui_info']") as XmlElement);

            // Load UI position
            if (int.TryParse((xmlDocument.SelectSingleNode(@"//int[@name='ui_position']") as XmlElement).GetAttribute("value"), out int ui_pos)) {
                this.Display.Position = ui_pos;
            }

            // Load Cost
            this.Cost = new(xmlDocument.SelectSingleNode(@"//group[@name='cost']") as XmlElement);
            if (this.Cost.IsNull) {
                this.Cost = null;
            }

            // Get activation
            this.Activation = (xmlDocument.SelectSingleNode(@"//enum[@name='activation']") as XmlElement).GetAttribute("value");

            // Load Requirements
            this.Requirements = Requirement.GetRequirements(xmlDocument.SelectSingleNode(@"//list[@name='requirements']") as XmlElement);

        }

    }

}

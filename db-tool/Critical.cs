using System.Xml;

namespace CoH2XML2JSON {

    public class Critical : BP {

        public override string ModGUID { get; }

        public override ulong PBGID { get; }

        public override string Name { get; }

        public UI Display { get; }

        public Critical(XmlDocument xmlDocument, string guid, string name) {

            // Set the name
            this.Name = name;

            // Set mod GUID
            this.ModGUID = string.IsNullOrEmpty(guid) ? null : guid;

            // Load pbgid
            this.PBGID = ulong.Parse(xmlDocument["instance"]["uniqueid"].GetAttribute("value"));

            // Load display
            this.Display = new(xmlDocument.SelectSingleNode(@"//template_reference[@name='ui_info']") as XmlElement);

        }

    }

}

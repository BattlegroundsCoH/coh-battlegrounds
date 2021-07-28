using System.ComponentModel;
using System.Xml;

namespace CoH2XML2JSON {
    
    public class WBP : BP {

        public override string ModGUID { get; }

        public override ulong PBGID { get; }

        public override string Name { get; }

        public UI Display { get; }

        [DefaultValue(0.0f)]
        public float Range { get; }

        [DefaultValue(0.0f)]
        public float Damage { get; }

        public WBP(XmlDocument xmlDocument, string guid, string name) {

            // Set the name
            this.Name = name;

            // Set mod GUID
            this.ModGUID = string.IsNullOrEmpty(guid) ? null : guid;

            // Load pbgid
            this.PBGID = ulong.Parse(xmlDocument["instance"]["uniqueid"].GetAttribute("value"));

            // Get range
            this.Range = float.Parse((xmlDocument.SelectSingleNode("//group[@name='range']") as XmlElement).FindSubnode("float", "max")?.GetAttribute("value") ?? "0");

            // Get damage
            this.Damage = float.Parse((xmlDocument.FirstChild as XmlElement).FindSubnode("group", "damage").FindSubnode("float", "max")?.GetAttribute("value") ?? "0");

        }

    }

}

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;

namespace CoH2XML2JSON {
    
    public class UBP : BP {

        public override string ModGUID { get; }

        public override ulong PBGID { get; }

        public override string Name { get; }

        public UI Display { get; }

        public Cost Cost { get; }

        [DefaultValue(null)]
        public string OwnerType { get; }

        [DefaultValue(null)]
        public string[] SlotItems { get; }

        [DefaultValue(null)]
        public Requirement[] Requirements { get; }

        public UBP(XmlDocument xmlDocument, string guid, string name) {

            // Set the name
            this.Name = name;

            // Set mod GUID
            this.ModGUID = string.IsNullOrEmpty(guid) ? null : guid;

            // Load pbgid
            this.PBGID = ulong.Parse(xmlDocument["instance"]["uniqueid"].GetAttribute("value"));

            // Load display
            this.Display = new(xmlDocument.SelectSingleNode("//group[@name='ui_info']") as XmlElement);

            // Load cost
            this.Cost = new(xmlDocument.SelectSingleNode("//group[@name='time_cost']") as XmlElement); 
            if (this.Cost.IsNull) {
                this.Cost = null;
            }

            // Get ownertype
            this.OwnerType = (xmlDocument.SelectSingleNode("//enum[@name='owner_type']") as XmlElement).GetAttribute("value");
            if (string.IsNullOrEmpty(this.OwnerType)) {
                this.OwnerType = null;
            }

            // Load slot items
            var slot_items = xmlDocument.SelectNodes("//instance_reference[@name='slot_item']");
            List<string> items = new();
            foreach (XmlElement item in slot_items) {
                items.Add(Path.GetFileNameWithoutExtension(item.GetAttribute("value")));
            }
            if (items.Count > 0) {
                this.SlotItems = items.ToArray();
            }

            // Load Requirements
            this.Requirements = Requirement.GetRequirements(xmlDocument.SelectSingleNode(@"//list[@name='requirements']") as XmlElement);

        }

    }

}

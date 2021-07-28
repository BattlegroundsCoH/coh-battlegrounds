using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace CoH2XML2JSON {
    
    public class ABP : BP {
    
        public Cost Cost { get; }

        public UI Display { get; }

        public string Army { get; init; }

        public override string ModGUID { get; }

        public override ulong PBGID { get; }

        public override string Name { get; }

        public ABP(XmlDocument xmlDocument, string guid, string name) {

            // Set the name
            this.Name = name;

            // Set mod GUID
            this.ModGUID = string.IsNullOrEmpty(guid) ? null : guid;

            // Load pbgid
            this.PBGID = ulong.Parse(xmlDocument["instance"]["uniqueid"].GetAttribute("value"));

            // Load display
            this.Display = new(xmlDocument.SelectSingleNode(@"//group[@name='ui_info']") as XmlElement);

            // Load Cost
            this.Cost = new(xmlDocument.SelectSingleNode(@"//group[@name='cost']") as XmlElement);

        }

    }

}

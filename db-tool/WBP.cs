using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace CoH2XML2JSON {
    
    public class WBP : BP {

        public override string ModGUID { get; }

        public override ulong PBGID { get; }

        public override string Name { get; }

        public UI Display { get; }

        public WBP(XmlDocument xmlDocument) {

        }

    }

}

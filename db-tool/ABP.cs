using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace CoH2XML2JSON {
    
    public class ABP : BP {
    
        public Cost Cost { get; }

        public UI Display { get; }

        public override string ModGUID { get; }

        public override ulong PBGID { get; }

        public override string Name { get; }

        public ABP(XmlDocument xmlDocument) {

        }

    }

}

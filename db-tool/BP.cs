using System.ComponentModel;

namespace CoH2XML2JSON {
    
    public abstract class BP {

        [DefaultValue(null)]
        public abstract string ModGUID { get; }

        public abstract ulong PBGID { get; }

        public abstract string Name { get; }

    }

}

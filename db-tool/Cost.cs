using System.Linq;
using System.Xml;

namespace CoH2XML2JSON {
    
    public class Cost {

        public float Manpower { get; }

        public float Munition { get; }

        public float Fuel { get; }

        public float FieldTime { get; }

        public bool IsNull => (this.Manpower + this.Munition + this.Fuel + this.FieldTime) == 0.0;

        public Cost(params Cost[] costs) {
            this.Manpower = costs.Sum(x => x.Manpower);
            this.Munition = costs.Sum(x => x.Munition);
            this.Fuel = costs.Sum(x => x.Fuel);
            this.FieldTime = costs.Sum(x => x.FieldTime);
        }

        public Cost(float man, float mun, float ful, float fld) {
            this.Manpower = man;
            this.Munition = mun;
            this.Fuel = ful;
            this.FieldTime = fld;
        }

        public Cost(XmlElement xmlElement) {
            if (xmlElement is not null) {
                this.Manpower = Program.GetFloat(xmlElement.FindSubnode("float", "manpower").GetAttribute("value"));
                this.Munition = Program.GetFloat(xmlElement.FindSubnode("float", "munition").GetAttribute("value"));
                this.Fuel = Program.GetFloat(xmlElement.FindSubnode("float", "fuel").GetAttribute("value"));
                this.FieldTime = Program.GetFloat(xmlElement.FindSubnode("float", "time_seconds")?.GetAttribute("value") ?? "0");
            }
        }

    }

}

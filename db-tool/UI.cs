using System.ComponentModel;
using System.Xml;

namespace CoH2XML2JSON {
    
    public class UI {

        public string LocaleName { get; }

        public string LocaleDescriptionLong { get; }

        [DefaultValue(null)]
        public string LocaleDescriptionShort { get; }

        [DefaultValue(null)]
        public string Icon { get; }

        [DefaultValue(null)]
        public string Symbol { get; }

        [DefaultValue(null)]
        public string Portrait { get; }

        [DefaultValue(0)]
        public int Position { get; set; }

        public UI(XmlElement xmlElement) {
            if (xmlElement is not null) {
                this.LocaleName = GetStr(xmlElement, "locstring", "screen_name");
                this.LocaleDescriptionLong = GetStr(xmlElement, "locstring", "help_text");
                this.LocaleDescriptionShort = GetStr(xmlElement, "locstring", "extra_text");
                this.Icon = xmlElement.FindSubnode("icon", "icon_name")?.GetAttribute("value") ?? null;
                if (string.IsNullOrEmpty(this.Icon)) {
                    this.Icon = null;
                }
                this.Symbol = xmlElement.FindSubnode("icon", "symbol_icon_name")?.GetAttribute("value") ?? null;
                this.Portrait = xmlElement.FindSubnode("icon", "portrait_name_summer")?.GetAttribute("value") ?? null;
            }
        }

        public static string GetStr(XmlElement xmlElement, string tag, string name) {
            var xmlNode = xmlElement.FindSubnode(tag, name);
            if (xmlNode is not null) {
                return GetStr(xmlNode);
            }
            return null;
        }

        public static string GetStr(XmlElement xmlElement) {
            string val = xmlElement.GetAttribute("value") ?? null;
            if (val is not null && xmlElement.GetAttribute("mod") is string mod && !string.IsNullOrEmpty(mod)) {
                val = $"${mod}:{val}";
            }
            return val;
        }

    }

}

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

        public UI(XmlElement xmlElement) {
            if (xmlElement is not null) {
                this.LocaleName = xmlElement.FindSubnode("locstring", "screen_name")?.GetAttribute("value") ?? null;
                this.LocaleDescriptionLong = xmlElement.FindSubnode("locstring", "help_text")?.GetAttribute("value") ?? null;
                this.LocaleDescriptionShort = xmlElement.FindSubnode("locstring", "extra_text")?.GetAttribute("value") ?? null;
                this.Icon = xmlElement.FindSubnode("icon", "icon_name")?.GetAttribute("value") ?? null;
                if (string.IsNullOrEmpty(this.Icon)) {
                    this.Icon = null;
                }
                this.Symbol = xmlElement.FindSubnode("icon", "symbol_icon_name")?.GetAttribute("value") ?? null;
                this.Portrait = xmlElement.FindSubnode("icon", "portrait_name_summer")?.GetAttribute("value") ?? null;
            }
        }

    }

}

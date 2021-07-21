using System.ComponentModel;
using System.Xml;

namespace CoH2XML2JSON {
    
    public class UI {

        public string LocaleName { get; }

        public string LocaleDescriptionLong { get; }

        [DefaultValue("")]
        public string LocaleDescriptionShort { get; }

        [DefaultValue("")]
        public string Icon { get; }

        [DefaultValue("")]
        public string Symbol { get; }

        [DefaultValue("")]
        public string Portrait { get; }

        public UI(XmlElement xmlElement) {
            if (xmlElement is not null) {
                this.LocaleName = xmlElement.FindSubnode("locstring", "screen_name")?.GetAttribute("value") ?? string.Empty;
                this.LocaleDescriptionLong = xmlElement.FindSubnode("locstring", "help_text")?.GetAttribute("value") ?? string.Empty;
                this.LocaleDescriptionShort = xmlElement.FindSubnode("locstring", "extra_text")?.GetAttribute("value") ?? string.Empty;
                this.Icon = xmlElement.FindSubnode("icon", "icon_name")?.GetAttribute("value") ?? string.Empty;
                this.Symbol = xmlElement.FindSubnode("icon", "symbol_icon_name")?.GetAttribute("value") ?? string.Empty;
                this.Portrait = xmlElement.FindSubnode("icon", "portrait_name_summer")?.GetAttribute("value") ?? string.Empty;
            }
        }

    }

}

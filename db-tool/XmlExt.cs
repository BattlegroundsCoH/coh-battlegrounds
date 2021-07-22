using System.Xml;

namespace CoH2XML2JSON {
    
    public static class XmlExt {

        public static string GetValue(this XmlElement element, string nodePath, string attribute = "value")
            => (element.SelectSingleNode(nodePath) as XmlElement).GetAttribute(attribute);

        public static XmlElement FindSubnode(this XmlElement e, string type, string name, string value) {
            foreach (XmlElement sub in e) {
                if (sub.Name == type && sub.HasAttribute("name") && sub.GetAttribute("name") == name && sub.HasAttribute("value") && sub.GetAttribute("value") == value) {
                    return sub;
                } else {
                    var subsub = FindSubnode(sub, type, name);
                    if (subsub is not null) {
                        return subsub;
                    }
                }
            }
            return null;
        }

        public static XmlElement FindSubnode(this XmlElement e, string type, string name) {
            foreach (XmlElement sub in e) {
                if (sub.Name == type && sub.HasAttribute("name") && sub.GetAttribute("name") == name) {
                    return sub;
                }
            }
            foreach (XmlElement sub in e) { // Call on children of elements (but top-layer has priority)
                var subsub = FindSubnode(sub, type, name);
                if (subsub is not null) {
                    return subsub;
                }
            }
            return null;
        }

    }

}

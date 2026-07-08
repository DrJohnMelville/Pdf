using System.Xml.Linq;

namespace Melville.Pdf.FormReader.AcroForms;

internal static class ElementByLocalNameImpl 
{ 
    extension (XElement elt)
    {
        public XElement? ElementByLocalName(string localName) => 
            elt.ElementsByLocalName(localName).FirstOrDefault();

        public IEnumerable<XElement> DescendantsByLocalName(string localName) =>
            elt.Descendants().FilterByLocalName(localName);
        public IEnumerable<XElement> ElementsByLocalName(string localName) =>
            elt.Elements().FilterByLocalName(localName);
    }

    extension (IEnumerable<XElement> seq)
    {
        public IEnumerable<XElement> FilterByLocalName(string localName) =>
            seq.Where(i => localName.Equals(i.Name.LocalName, StringComparison.Ordinal));
    }
}
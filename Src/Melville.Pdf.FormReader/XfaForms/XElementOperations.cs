using System.Xml.Linq;

namespace Melville.Pdf.FormReader.XfaForms
{
    internal static class XElementOperations
    {
        public static string InnerText(this XElement xmlElement) => 
            string.Join("", xmlElement.Nodes().OfType<XText>().Select(i=>i.Value));
    }
}
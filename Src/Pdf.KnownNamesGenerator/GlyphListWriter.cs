using System.Collections.Generic;
using System.Text;
using Pdf.KnownNamesGenerator.KnownNames;

namespace Pdf.KnownNamesGenerator;

public class GlyphListWriter
{
    private StringBuilder output = new();
    private IDictionary<string, string> source;

    public GlyphListWriter(IDictionary<string, string> source)
    {
        this.source = source;
    }

    public string Write()
    {
        WritePrefix();
        WriteElements();
        WriteSuffix();
        return output.ToString();
    }

    private void WriteElements()
    {
        foreach (var item in source)
        {
            WriteSingleElement(item);
        }
    }

    private void WriteSingleElement(KeyValuePair<string, string> item)
    {
        output.AppendLine($"        {{{Fnv.FromString(item.Key)},(char)0x{item.Value}}}, // {item.Key}");
    }

    private void WritePrefix()
    {
        output.AppendLine("using System.Collections.Generic;");
        output.AppendLine("namespace Melville.Pdf.LowLevel.Model.CharacterEncoding;");
        output.AppendLine();
        output.AppendLine("public partial class GlyphNameToUnicodeMap");
        output.AppendLine("{");
        output.AppendLine("    public static readonly GlyphNameToUnicodeMap AdobeGlyphList = new(new Dictionary <int, char>()");
        output.AppendLine("    {");
        
    }

    private void WriteSuffix()
    {
        output.AppendLine("    });");
        output.AppendLine("}");
    }
}
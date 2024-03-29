﻿using System.Collections.Generic;
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
        output.AppendLine($"        {{{Fnv.FromString(item.Key)},0x{item.Value}}}, // {item.Key}");
    }
     
    private void WritePrefix()
    {
        output.AppendLine("using System.Collections.Generic;");
        output.AppendLine("namespace Melville.Pdf.LowLevel.Model.CharacterEncoding;");
        output.AppendLine();
        output.AppendLine("/// <summary>");
        output.AppendLine("/// This class maps glyph names onto their unicode equivilents.  right now Only the Adobe Glyph List is Supported.");
        output.AppendLine("/// </summary>");
        output.AppendLine("public static class GlyphNameToUnicodeMap");
        output.AppendLine("{");
        output.AppendLine("/// <summary>");
        output.AppendLine("/// This class converts FNV hashes of names in the Adobe Glyph list to ");
        output.AppendLine("/// the corresponding unicode code point.");
        output.AppendLine("/// </summary>");
        output.AppendLine("    public static readonly INameToGlyphMapping AdobeGlyphList = new DictionaryGlyphNameMapper(new Dictionary <uint, uint>()");
        output.AppendLine("    {");
        
    }

    private void WriteSuffix()
    {
        output.AppendLine("    });");
        output.AppendLine("}");
    }
}
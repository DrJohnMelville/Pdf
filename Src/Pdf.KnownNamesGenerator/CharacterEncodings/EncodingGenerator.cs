using System;
using System.Collections.Generic;
using System.Text;

namespace Pdf.KnownNamesGenerator.CharacterEncodings;

public class EncodingGenerator
{
    private IDictionary<string, string> glyphNameToUnicode;
    private MultiEncodingMaps maps;
    private IReadOnlyDictionary<byte, string> symbolMap;
    private IReadOnlyDictionary<byte, string> macExpertMap;
    public EncodingGenerator(IDictionary<string, string> glyphNameToUnicode, 
        MultiEncodingMaps maps, IReadOnlyDictionary<byte, string> symbolMap, 
        IReadOnlyDictionary<byte, string> macExpertMap)
    {
        this.glyphNameToUnicode = glyphNameToUnicode;
        this.maps = maps;
        this.symbolMap = symbolMap;
        this.macExpertMap = macExpertMap;
    }

    public string Generate()
    {
        var sb = new StringBuilder();
        GenerateAllEncodings(sb);
        var text = sb.ToString();
        UdpConsole.WriteLine(text.Substring(0,1000));
        return text;
    }

    private void GenerateAllEncodings(StringBuilder sb)
    {
        GeneratePreamble(sb);
        GenerateEncoding(sb,"MacExpert", macExpertMap);
        GenerateEncoding(sb,"Symbol", symbolMap);
        GenerateEncoding(sb,"Standard",maps.Standard);
        GenerateEncoding(sb,"WinAnsi",maps.Win);
        GenerateEncoding(sb,"MacRoman",maps.Mac);
        GenerateEncoding(sb,"Pdf",maps.Pdf);
        CloseClassBlock(sb);
    }

    private void GeneratePreamble(StringBuilder sb)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine("namespace Melville.Pdf.LowLevel.Model.CharacterEncoding;");
        sb.AppendLine();
        sb.AppendLine("public static partial class CharacterEncodings {");
    }

    private void CloseClassBlock(StringBuilder sb)
    {
        sb.AppendLine("}");
    }

    private void GenerateEncoding(StringBuilder sb, string name, IReadOnlyDictionary<byte, string> map)
    {
        sb.AppendLine($"    public static IByteToUnicodeMapping {name} = new TableMapping(new char[] {{");
        sb.Append("        ");
        for (int i = 0; i < 256; i++)
        {
            var value = ComputeUnicodeForGlyph(map, i);
            sb.Append($" (char)(0x{value}),");
        }
        sb.AppendLine("    });");
    }

    private string ComputeUnicodeForGlyph(IReadOnlyDictionary<byte, string> map, int i)
    {
        if (!map.TryGetValue((byte)i, out var mappedName)) return "25CF";
        if (!glyphNameToUnicode.TryGetValue(mappedName, out var uni))
            throw new InvalidOperationException("Cannot find glyph named: " + mappedName);
        return uni;
    }
}
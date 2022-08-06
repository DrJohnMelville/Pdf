using System;
using System.Collections.Generic;
using System.Linq;
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
        return text;
    }

    private void GenerateAllEncodings(StringBuilder sb)
    {
        GeneratePreamble(sb);
        GenerateEncodingClass(sb);
    }

    private void GenerateEncodingClass(StringBuilder sb)
    {
        GeneraterEncodingClassBlock(sb);
        GenerateEncoding(sb, "MacExpert", macExpertMap);
        GenerateEncoding(sb, "Symbol", symbolMap);
        GenerateEncoding(sb, "Standard", maps.Standard);
        GenerateEncoding(sb, "WinAnsi", maps.Win);
        GenerateEncoding(sb, "MacRoman", maps.Mac);
        GenerateEncoding(sb, "Pdf", maps.Pdf);
        CloseClassBlock(sb);
        
        GenerateNamesClass(sb);
    }
    
    private void GeneratePreamble(StringBuilder sb)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine("namespace Melville.Pdf.LowLevel.Model.CharacterEncoding;");
        sb.AppendLine();
    }

    private static void GeneraterEncodingClassBlock(StringBuilder sb)
    {
        sb.AppendLine("public static partial class CharacterEncodings {");
    }

    private void CloseClassBlock(StringBuilder sb)
    {
        sb.AppendLine("}");
    }

    private void GenerateEncoding(StringBuilder sb, string name, IReadOnlyDictionary<byte, string> map)
    {
        sb.AppendLine($"    public static byte[][] {name} = {{");
        for (int i = 0; i < 256; i++)
        {
            var value = ComputeUnicodeForGlyph(map, i);
            sb.AppendLine($"        RomanCharacterNames.{value},");
        }
        sb.AppendLine("    };");
    }

    private string ComputeUnicodeForGlyph(IReadOnlyDictionary<byte, string> map, int i)
    {
        if (!map.TryGetValue((byte)i, out var mappedName)) return "notdef";
        return mappedName;
    }
    
    private void GenerateNamesClass(StringBuilder sb)
    {
        sb.AppendLine("public static class RomanCharacterNames");
        sb.AppendLine("{");
        foreach (var name in AllCharacterNames())
        {
            sb.Append("    public static readonly byte[] ");
            sb.Append(name);
            sb.Append(" = {");
            OutputAsBytes(sb, name);
            sb.AppendLine("};");
        }
        sb.AppendLine("}");
    }

    private IEnumerable<string> AllCharacterNames()
    {
        return new[]
        {
            macExpertMap,
            symbolMap,
            maps.Standard,
            maps.Win,
            maps.Mac,
            maps.Pdf
        }.SelectMany(i => i.Values)
            .Prepend("notdef")
            .Distinct();
    }

    private static void OutputAsBytes(StringBuilder sb, string name)
    {
        foreach (var character in name)
        {
            sb.Append((int)character);
            sb.Append(',');
        }
    }
}
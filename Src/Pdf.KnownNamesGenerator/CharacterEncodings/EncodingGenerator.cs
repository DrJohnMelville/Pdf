using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pdf.KnownNamesGenerator.CharacterEncodings;

public class EncodingGenerator
{
    private MultiEncodingMaps maps;
    private IReadOnlyDictionary<byte, string> symbolMap;
    private IReadOnlyDictionary<byte, string> macExpertMap;
    public EncodingGenerator(MultiEncodingMaps maps, IReadOnlyDictionary<byte, string> symbolMap, 
        IReadOnlyDictionary<byte, string> macExpertMap)
    {
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
        GenerateEncoding(sb, "ZapfDingbats", zapfDingbatsMap);
        CloseClassBlock(sb);
        
        GenerateNamesClass(sb);
    }
    
    private void GeneratePreamble(StringBuilder sb)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Melville.Pdf.LowLevel.Model.Objects;");
        sb.AppendLine("namespace Melville.Pdf.LowLevel.Model.CharacterEncoding;");
        sb.AppendLine("#pragma warning disable CS1591");
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
        sb.AppendLine($"    public static PdfDirectValue[] {name} = {{");
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
            sb.Append("    public static readonly PdfDirectValue ");
            sb.Append(name);
            sb.Append(" = PdfDirectValue.CreateName(\"");
            sb.Append(name);
            sb.AppendLine("\"u8);");
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
            maps.Pdf,
            zapfDingbatsMap
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
    
        private IReadOnlyDictionary<byte, string> zapfDingbatsMap = new Dictionary<byte, string>()
    {
            {0x20, "space"}, // SPACE
            {0x21, "a1"}, // UPPER BLADE SCISSORS
            {0x22, "a2"}, // BLACK SCISSORS
            {0x23, "a202"}, // LOWER BLADE SCISSORS
            {0x24, "a3"}, // WHITE SCISSORS
            {0x25, "a4"}, // BLACK TELEPHONE
            {0x26, "a5"}, // TELEPHONE LOCATION SIGN
            {0x27, "a119"}, // TAPE DRIVE
            {0x28, "a118"}, // AIRPLANE
            {0x29, "a117"}, // ENVELOPE
            {0x2A, "a11"}, // BLACK RIGHT POINTING INDEX
            {0x2B, "a12"}, // WHITE RIGHT POINTING INDEX
            {0x2C, "a13"}, // VICTORY HAND
            {0x2D, "a14"}, // WRITING HAND
            {0x2E, "a15"}, // LOWER RIGHT PENCIL
            {0x2F, "a16"}, // PENCIL
            {0x30, "a105"}, // UPPER RIGHT PENCIL
            {0x31, "a17"}, // WHITE NIB
            {0x32, "a18"}, // BLACK NIB
            {0x33, "a19"}, // CHECK MARK
            {0x34, "a20"}, // HEAVY CHECK MARK
            {0x35, "a21"}, // MULTIPLICATION X
            {0x36, "a22"}, // HEAVY MULTIPLICATION X
            {0x37, "a23"}, // BALLOT X
            {0x38, "a24"}, // HEAVY BALLOT X
            {0x39, "a25"}, // OUTLINED GREEK CROSS
            {0x3A, "a26"}, // HEAVY GREEK CROSS
            {0x3B, "a27"}, // OPEN CENTRE CROSS
            {0x3C, "a28"}, // HEAVY OPEN CENTRE CROSS
            {0x3D, "a6"}, // LATIN CROSS
            {0x3E, "a7"}, // SHADOWED WHITE LATIN CROSS
            {0x3F, "a8"}, // OUTLINED LATIN CROSS
            {0x40, "a9"}, // MALTESE CROSS
            {0x41, "a10"}, // STAR OF DAVID
            {0x42, "a29"}, // FOUR TEARDROP-SPOKED ASTERISK
            {0x43, "a30"}, // FOUR BALLOON-SPOKED ASTERISK
            {0x44, "a31"}, // HEAVY FOUR BALLOON-SPOKED ASTERISK
            {0x45, "a32"}, // FOUR CLUB-SPOKED ASTERISK
            {0x46, "a33"}, // BLACK FOUR POINTED STAR
            {0x47, "a34"}, // WHITE FOUR POINTED STAR
            {0x48, "a35"}, // BLACK STAR
            {0x49, "a36"}, // STRESS OUTLINED WHITE STAR
            {0x4A, "a37"}, // CIRCLED WHITE STAR
            {0x4B, "a38"}, // OPEN CENTRE BLACK STAR
            {0x4C, "a39"}, // BLACK CENTRE WHITE STAR
            {0x4D, "a40"}, // OUTLINED BLACK STAR
            {0x4E, "a41"}, // HEAVY OUTLINED BLACK STAR
            {0x4F, "a42"}, // PINWHEEL STAR
            {0x50, "a43"}, // SHADOWED WHITE STAR
            {0x51, "a44"}, // HEAVY ASTERISK
            {0x52, "a45"}, // OPEN CENTRE ASTERISK
            {0x53, "a46"}, // EIGHT SPOKED ASTERISK
            {0x54, "a47"}, // EIGHT POINTED BLACK STAR
            {0x55, "a48"}, // EIGHT POINTED PINWHEEL STAR
            {0x56, "a49"}, // SIX POINTED BLACK STAR
            {0x57, "a50"}, // EIGHT POINTED RECTILINEAR BLACK STAR
            {0x58, "a51"}, // HEAVY EIGHT POINTED RECTILINEAR BLACK STAR
            {0x59, "a52"}, // TWELVE POINTED BLACK STAR
            {0x5A, "a53"}, // SIXTEEN POINTED ASTERISK
            {0x5B, "a54"}, // TEARDROP-SPOKED ASTERISK
            {0x5C, "a55"}, // OPEN CENTRE TEARDROP-SPOKED ASTERISK
            {0x5D, "a56"}, // HEAVY TEARDROP-SPOKED ASTERISK
            {0x5E, "a57"}, // SIX PETALLED BLACK AND WHITE FLORETTE
            {0x5F, "a58"}, // BLACK FLORETTE
            {0x60, "a59"}, // WHITE FLORETTE
            {0x61, "a60"}, // EIGHT PETALLED OUTLINED BLACK FLORETTE
            {0x62, "a61"}, // CIRCLED OPEN CENTRE EIGHT POINTED STAR
            {0x63, "a62"}, // HEAVY TEARDROP-SPOKED PINWHEEL ASTERISK
            {0x64, "a63"}, // SNOWFLAKE
            {0x65, "a64"}, // TIGHT TRIFOLIATE SNOWFLAKE
            {0x66, "a65"}, // HEAVY CHEVRON SNOWFLAKE
            {0x67, "a66"}, // SPARKLE
            {0x68, "a67"}, // HEAVY SPARKLE
            {0x69, "a68"}, // BALLOON-SPOKED ASTERISK
            {0x6A, "a69"}, // EIGHT TEARDROP-SPOKED PROPELLER ASTERISK
            {0x6B, "a70"}, // HEAVY EIGHT TEARDROP-SPOKED PROPELLER ASTERISK
            {0x6C, "a71"}, // BLACK CIRCLE
            {0x6D, "a72"}, // SHADOWED WHITE CIRCLE
            {0x6E, "a73"}, // BLACK SQUARE
            {0x6F, "a74"}, // LOWER RIGHT DROP-SHADOWED WHITE SQUARE
            {0x70, "a203"}, // UPPER RIGHT DROP-SHADOWED WHITE SQUARE
            {0x71, "a75"}, // LOWER RIGHT SHADOWED WHITE SQUARE
            {0x72, "a204"}, // UPPER RIGHT SHADOWED WHITE SQUARE
            {0x73, "a76"}, // BLACK UP-POINTING TRIANGLE
            {0x74, "a77"}, // BLACK DOWN-POINTING TRIANGLE
            {0x75, "a78"}, // BLACK DIAMOND
            {0x76, "a79"}, // BLACK DIAMOND MINUS WHITE X
            {0x77, "a81"}, // RIGHT HALF BLACK CIRCLE
            {0x78, "a82"}, // LIGHT VERTICAL BAR
            {0x79, "a83"}, // MEDIUM VERTICAL BAR
            {0x7A, "a84"}, // HEAVY VERTICAL BAR
            {0x7B, "a97"}, // HEAVY SINGLE TURNED COMMA QUOTATION MARK ORNAMENT
            {0x7C, "a98"}, // HEAVY SINGLE COMMA QUOTATION MARK ORNAMENT
            {0x7D, "a99"}, // HEAVY DOUBLE TURNED COMMA QUOTATION MARK ORNAMENT
            {0x7E, "a100"}, // HEAVY DOUBLE COMMA QUOTATION MARK ORNAMENT
            {0x80, "a89"}, // MEDIUM LEFT PARENTHESIS ORNAMENT
            {0x81, "a90"}, // MEDIUM RIGHT PARENTHESIS ORNAMENT
            {0x82, "a93"}, // MEDIUM FLATTENED LEFT PARENTHESIS ORNAMENT
            {0x83, "a94"}, // MEDIUM FLATTENED RIGHT PARENTHESIS ORNAMENT
            {0x84, "a91"}, // MEDIUM LEFT-POINTING ANGLE BRACKET ORNAMENT
            {0x85, "a92"}, // MEDIUM RIGHT-POINTING ANGLE BRACKET ORNAMENT
            {0x86, "a205"}, // HEAVY LEFT-POINTING ANGLE QUOTATION MARK ORNAMENT
            {0x87, "a85"}, // HEAVY RIGHT-POINTING ANGLE QUOTATION MARK ORNAMENT
            {0x88, "a206"}, // HEAVY LEFT-POINTING ANGLE BRACKET ORNAMENT
            {0x89, "a86"}, // HEAVY RIGHT-POINTING ANGLE BRACKET ORNAMENT
            {0x8A, "a87"}, // LIGHT LEFT TORTOISE SHELL BRACKET ORNAMENT
            {0x8B, "a88"}, // LIGHT RIGHT TORTOISE SHELL BRACKET ORNAMENT
            {0x8C, "a95"}, // MEDIUM LEFT CURLY BRACKET ORNAMENT
            {0x8D, "a96"}, // MEDIUM RIGHT CURLY BRACKET ORNAMENT
            {0xA1, "a101"}, // CURVED STEM PARAGRAPH SIGN ORNAMENT
            {0xA2, "a102"}, // HEAVY EXCLAMATION MARK ORNAMENT
            {0xA3, "a103"}, // HEAVY HEART EXCLAMATION MARK ORNAMENT
            {0xA4, "a104"}, // HEAVY BLACK HEART
            {0xA5, "a106"}, // ROTATED HEAVY BLACK HEART BULLET
            {0xA6, "a107"}, // FLORAL HEART
            {0xA7, "a108"}, // ROTATED FLORAL HEART BULLET
            {0xA8, "a112"}, // BLACK CLUB SUIT
            {0xA9, "a111"}, // BLACK DIAMOND SUIT
            {0xAA, "a110"}, // BLACK HEART SUIT
            {0xAB, "a109"}, // BLACK SPADE SUIT
            {0xAC, "a120"}, // CIRCLED DIGIT ONE
            {0xAD, "a121"}, // CIRCLED DIGIT TWO
            {0xAE, "a122"}, // CIRCLED DIGIT THREE
            {0xAF, "a123"}, // CIRCLED DIGIT FOUR
            {0xB0, "a124"}, // CIRCLED DIGIT FIVE
            {0xB1, "a125"}, // CIRCLED DIGIT SIX
            {0xB2, "a126"}, // CIRCLED DIGIT SEVEN
            {0xB3, "a127"}, // CIRCLED DIGIT EIGHT
            {0xB4, "a128"}, // CIRCLED DIGIT NINE
            {0xB5, "a129"}, // CIRCLED NUMBER TEN
            {0xB6, "a130"}, // DINGBAT NEGATIVE CIRCLED DIGIT ONE
            {0xB7, "a131"}, // DINGBAT NEGATIVE CIRCLED DIGIT TWO
            {0xB8, "a132"}, // DINGBAT NEGATIVE CIRCLED DIGIT THREE
            {0xB9, "a133"}, // DINGBAT NEGATIVE CIRCLED DIGIT FOUR
            {0xBA, "a134"}, // DINGBAT NEGATIVE CIRCLED DIGIT FIVE
            {0xBB, "a135"}, // DINGBAT NEGATIVE CIRCLED DIGIT SIX
            {0xBC, "a136"}, // DINGBAT NEGATIVE CIRCLED DIGIT SEVEN
            {0xBD, "a137"}, // DINGBAT NEGATIVE CIRCLED DIGIT EIGHT
            {0xBE, "a138"}, // DINGBAT NEGATIVE CIRCLED DIGIT NINE
            {0xBF, "a139"}, // DINGBAT NEGATIVE CIRCLED NUMBER TEN
            {0xC0, "a140"}, // DINGBAT CIRCLED SANS-SERIF DIGIT ONE
            {0xC1, "a141"}, // DINGBAT CIRCLED SANS-SERIF DIGIT TWO
            {0xC2, "a142"}, // DINGBAT CIRCLED SANS-SERIF DIGIT THREE
            {0xC3, "a143"}, // DINGBAT CIRCLED SANS-SERIF DIGIT FOUR
            {0xC4, "a144"}, // DINGBAT CIRCLED SANS-SERIF DIGIT FIVE
            {0xC5, "a145"}, // DINGBAT CIRCLED SANS-SERIF DIGIT SIX
            {0xC6, "a146"}, // DINGBAT CIRCLED SANS-SERIF DIGIT SEVEN
            {0xC7, "a147"}, // DINGBAT CIRCLED SANS-SERIF DIGIT EIGHT
            {0xC8, "a148"}, // DINGBAT CIRCLED SANS-SERIF DIGIT NINE
            {0xC9, "a149"}, // DINGBAT CIRCLED SANS-SERIF NUMBER TEN
            {0xCA, "a150"}, // DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT ONE
            {0xCB, "a151"}, // DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT TWO
            {0xCC, "a152"}, // DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT THREE
            {0xCD, "a153"}, // DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT FOUR
            {0xCE, "a154"}, // DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT FIVE
            {0xCF, "a155"}, // DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT SIX
            {0xD0, "a156"}, // DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT SEVEN
            {0xD1, "a157"}, // DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT EIGHT
            {0xD2, "a158"}, // DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT NINE
            {0xD3, "a159"}, // DINGBAT NEGATIVE CIRCLED SANS-SERIF NUMBER TEN
            {0xD4, "a160"}, // HEAVY WIDE-HEADED RIGHTWARDS ARROW
            {0xD5, "a161"}, // RIGHTWARDS ARROW
            {0xD6, "a163"}, // LEFT RIGHT ARROW
            {0xD7, "a164"}, // UP DOWN ARROW
            {0xD8, "a196"}, // HEAVY SOUTH EAST ARROW
            {0xD9, "a165"}, // HEAVY RIGHTWARDS ARROW
            {0xDA, "a192"}, // HEAVY NORTH EAST ARROW
            {0xDB, "a166"}, // DRAFTING POINT RIGHTWARDS ARROW
            {0xDC, "a167"}, // HEAVY ROUND-TIPPED RIGHTWARDS ARROW
            {0xDD, "a168"}, // TRIANGLE-HEADED RIGHTWARDS ARROW
            {0xDE, "a169"}, // HEAVY TRIANGLE-HEADED RIGHTWARDS ARROW
            {0xDF, "a170"}, // DASHED TRIANGLE-HEADED RIGHTWARDS ARROW
            {0xE0, "a171"}, // HEAVY DASHED TRIANGLE-HEADED RIGHTWARDS ARROW
            {0xE1, "a172"}, // BLACK RIGHTWARDS ARROW
            {0xE2, "a173"}, // THREE-D TOP-LIGHTED RIGHTWARDS ARROWHEAD
            {0xE3, "a162"}, // THREE-D BOTTOM-LIGHTED RIGHTWARDS ARROWHEAD
            {0xE4, "a174"}, // BLACK RIGHTWARDS ARROWHEAD
            {0xE5, "a175"}, // HEAVY BLACK CURVED DOWNWARDS AND RIGHTWARDS ARROW
            {0xE6, "a176"}, // HEAVY BLACK CURVED UPWARDS AND RIGHTWARDS ARROW
            {0xE7, "a177"}, // SQUAT BLACK RIGHTWARDS ARROW
            {0xE8, "a178"}, // HEAVY CONCAVE-POINTED BLACK RIGHTWARDS ARROW
            {0xE9, "a179"}, // RIGHT-SHADED WHITE RIGHTWARDS ARROW
            {0xEA, "a193"}, // LEFT-SHADED WHITE RIGHTWARDS ARROW
            {0xEB, "a180"}, // BACK-TILTED SHADOWED WHITE RIGHTWARDS ARROW
            {0xEC, "a199"}, // FRONT-TILTED SHADOWED WHITE RIGHTWARDS ARROW
            {0xED, "a181"}, // HEAVY LOWER RIGHT-SHADOWED WHITE RIGHTWARDS ARROW
            {0xEE, "a200"}, // HEAVY UPPER RIGHT-SHADOWED WHITE RIGHTWARDS ARROW
            {0xEF, "a182"}, // NOTCHED LOWER RIGHT-SHADOWED WHITE RIGHTWARDS ARROW
            {0xF1, "a201"}, // NOTCHED UPPER RIGHT-SHADOWED WHITE RIGHTWARDS ARROW
            {0xF2, "a183"}, // CIRCLED HEAVY WHITE RIGHTWARDS ARROW
            {0xF3, "a184"}, // WHITE-FEATHERED RIGHTWARDS ARROW
            {0xF4, "a197"}, // BLACK-FEATHERED SOUTH EAST ARROW
            {0xF5, "a185"}, // BLACK-FEATHERED RIGHTWARDS ARROW
            {0xF6, "a194"}, // BLACK-FEATHERED NORTH EAST ARROW
            {0xF7, "a198"}, // HEAVY BLACK-FEATHERED SOUTH EAST ARROW
            {0xF8, "a186"}, // HEAVY BLACK-FEATHERED RIGHTWARDS ARROW
            {0xF9, "a195"}, // HEAVY BLACK-FEATHERED NORTH EAST ARROW
            {0xFA, "a187"}, // TEARDROP-BARBED RIGHTWARDS ARROW
            {0xFB, "a188"}, // HEAVY TEARDROP-SHANKED RIGHTWARDS ARROW
            {0xFC, "a189"}, // WEDGE-TAILED RIGHTWARDS ARROW
            {0xFD, "a190"}, // HEAVY WEDGE-TAILED RIGHTWARDS ARROW
            {0xFE, "a191"}, // OPEN-OUTLINED RIGHTWARDS ARROW
    };
}
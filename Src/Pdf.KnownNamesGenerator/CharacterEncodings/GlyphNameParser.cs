using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pdf.KnownNamesGenerator.CharacterEncodings;

public static class GlyphNameParser
{
    public static IDictionary<string,string> Parse(string source) => 
        ExtractPairs(source)
            .ToDictionary(match => match.Groups[1].Value, match => match.Groups[2].Value);

    private static IEnumerable<Match> ExtractPairs(string source) => 
        Regex.Matches(source, @"^([^;\s]+);([0-9A-F]+)\s*$", RegexOptions.Multiline)
            .Cast<Match>();
}
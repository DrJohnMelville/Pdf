using System;
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
        Regex.Matches(source, @"^([^;\s]+);([0-9A-Fa-f]+)\s*$", RegexOptions.Multiline)
            .Cast<Match>();
}

public static class SimpleMapParser
{
    public static IReadOnlyDictionary<byte, string> Parse(string source) =>
        source.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(i => i.Split(';'))
            .ToDictionary(KeySelector, i=>i[0]);

    private static byte KeySelector(string[] i)
    {
        var ret = (byte)ParseOctal(i[1]);
        return ret;
    }

    private static int ParseOctal(string octal){
        int ret = 0;
        foreach(char c in octal){
            ret *= 8;
            ret += c - '0';
        }
        return ret;
    }
}
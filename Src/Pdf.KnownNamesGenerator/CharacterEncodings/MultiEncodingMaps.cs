using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Pdf.KnownNamesGenerator.CharacterEncodings;

public class MultiEncodingMaps
{
    public Dictionary<byte, string> Standard = new();
    public Dictionary<byte, string> Mac = new();
    public Dictionary<byte, string> Win = new();
    public Dictionary<byte, string> Pdf = new();

    public MultiEncodingMaps(string inputfile)
    {
        foreach (var row in GetRows(inputfile))
        {
            TryAddValue(Standard, row[1], row[2]);
            TryAddValue(Mac, row[1], row[3]);
            TryAddValue(Win, row[1], row[4]);
            TryAddValue(Pdf, row[1], row[5]);
        }
    }

    private void TryAddValue(Dictionary<byte,string> dictionary, string name, string code)
    {
        if (CharacterNotMappedInThisEncoding(code)) return;
        dictionary.Add(byte.Parse(code, NumberStyles.AllowHexSpecifier), name);
    }

    private static bool CharacterNotMappedInThisEncoding(string code) => code.Length > 3;

    private static IEnumerable<string[]> GetRows(string inputfile) =>
        inputfile
            .Split(new[]{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Select(i=>i.Split(','));
}
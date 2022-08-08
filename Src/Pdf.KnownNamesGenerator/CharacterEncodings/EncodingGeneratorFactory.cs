using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Pdf.KnownNamesGenerator.CharacterEncodings;

public static class EncodingGeneratorFactory
{
    public static EncodingGenerator Create(ImmutableArray<AdditionalText> texts)
    {
        return new EncodingGenerator(
            new MultiEncodingMaps(Inputfile(texts, "stdEncodings.cedsl")),
            SimpleMapParser.Parse(Inputfile(texts, "Symbol.cedsl")),
            SimpleMapParser.Parse(Inputfile(texts, "MacExpert.cedsl")));
    }

    private static string Inputfile(ImmutableArray<AdditionalText> texts, string name) => 
        texts.First(i => i.Path.EndsWith(name)).GetText()!.ToString();
}
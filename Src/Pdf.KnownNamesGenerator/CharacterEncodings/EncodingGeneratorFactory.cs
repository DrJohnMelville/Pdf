using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Pdf.KnownNamesGenerator.CharacterEncodings;

public static class EncodingGeneratorFactory
{
    public static EncodingGenerator Create(ImmutableArray<AdditionalText> texts)
    {
        return new EncodingGenerator(GlyphNameParser.Parse(Inputfile(texts, "glyphlist.cedsl")),
            new MultiEncodingMaps(Inputfile(texts, "stdEncodings.cedsl")),
            SimpleMapParser.Parse(Inputfile(texts, "Symbol.cedsl")),
            SimpleMapParser.Parse(Inputfile(texts, "MacExpert.cedsl")));
    }

    private static string Inputfile(ImmutableArray<AdditionalText> texts, string name) => 
        texts.First(i => i.Path.EndsWith(name)).GetText()!.ToString();
}

#warning get rid of this when done with generator
public static class UdpConsole
{
    private static UdpClient? client = null;
    private static UdpClient Client
    {
        get
        {
            client ??= new UdpClient();
            return client ;
        }
    }

    public static string WriteLine(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        Client.Send(bytes, bytes.Length, "127.0.0.1", 15321);
        return str;
    }
}
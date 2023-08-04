using System.Diagnostics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

internal readonly partial struct SingleByteEncodingParser
{
    [FromConstructor] private readonly INameToGlyphMapping nameMapper;
    [FromConstructor] private readonly uint[] output;
    [FromConstructor] private readonly PdfDirectObject[]? overrideEncoding;

#if DEBUG
    partial void OnConstructed()
    {
        Debug.Assert(output.Length == 256);
    }
#endif

    public async ValueTask WriteEncodingToArrayAsync(PdfDirectObject encoding)
    {
        switch (encoding)
        {
            case {IsName:true} name:
                WriteCharacterSet(overrideEncoding ?? CharactersFromName(name));
                break;
            case var x when x.TryGet(out PdfDictionary dict):
                await WriteEncodingToArrayAsync(await dict.GetOrNullAsync(KnownNames.BaseEncoding).CA()).CA();
                if ((await dict.GetOrNullAsync<PdfArray>(KnownNames.Differences).CA()) is {} differences )
                    await WriteDifferencesAsync(differences).CA();
                break; 
            default:
                WriteCharacterSet(overrideEncoding?? CharacterEncodings.Standard);
                break;
        }
    }
    
    private PdfDirectObject[] CharactersFromName(PdfDirectObject name) => 
        name switch
    {
        var x when x.Equals(KnownNames.MacRomanEncoding) => CharacterEncodings.MacRoman,
        var x when x.Equals(KnownNames.MacExpertEncoding) => CharacterEncodings.MacExpert,
        var x when x.Equals(KnownNames.Symbol) => CharacterEncodings.Symbol,
        var x when x.Equals(KnownNames.PdfDocEncoding) => CharacterEncodings.Pdf,
        var x when x.Equals(KnownNames.WinAnsiEncoding) => CharacterEncodings.WinAnsi,
        _ => CharacterEncodings.Standard
    };

    private void WriteCharacterSet(PdfDirectObject[] characters)
    {
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = nameMapper.GetGlyphFor(characters[i]);
        }
    }
    
    private async ValueTask WriteDifferencesAsync(PdfArray differences)
    {
        byte currentChar = 0;
        await foreach (var item in differences.CA())
        {
            switch (item)
            {
                case {IsName:true} name:
                    var glyph = nameMapper.GetGlyphFor(name);
                    if (glyph > 0) output[currentChar++] = glyph;
                    break;
                case var x when x.TryGet(out long num):
                    currentChar = (byte)num;
                    break;
            }
        }
    }

}
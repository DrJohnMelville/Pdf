using System.Diagnostics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

internal readonly partial struct SingleByteEncodingParser
{
    [FromConstructor] private readonly INameToGlyphMapping nameMapper;
    [FromConstructor] private readonly uint[] output;
    [FromConstructor] private readonly PdfDirectValue[]? overrideEncoding;

#if DEBUG
    partial void OnConstructed()
    {
        Debug.Assert(output.Length == 256);
    }
#endif

    public async ValueTask WriteEncodingToArrayAsync(PdfDirectValue encoding)
    {
        switch (encoding)
        {
            case {IsName:true} name:
                WriteCharacterSet(overrideEncoding ?? CharactersFromName(name));
                break;
            case var x when x.TryGet(out PdfValueDictionary dict):
                await WriteEncodingToArrayAsync(await dict.GetOrNullAsync(KnownNames.BaseEncodingTName).CA()).CA();
                if ((await dict.GetOrNullAsync<PdfValueArray>(KnownNames.DifferencesTName).CA()) is {} differences )
                    await WriteDifferencesAsync(differences).CA();
                break; 
            default:
                WriteCharacterSet(overrideEncoding?? CharacterEncodings.Standard);
                break;
        }
    }
    
    private PdfDirectValue[] CharactersFromName(PdfDirectValue name) => 
        name switch
    {
        var x when x.Equals(KnownNames.MacRomanEncodingTName) => CharacterEncodings.MacRoman,
        var x when x.Equals(KnownNames.MacExpertEncodingTName) => CharacterEncodings.MacExpert,
        var x when x.Equals(KnownNames.SymbolTName) => CharacterEncodings.Symbol,
        var x when x.Equals(KnownNames.PdfDocEncodingTName) => CharacterEncodings.Pdf,
        var x when x.Equals(KnownNames.WinAnsiEncodingTName) => CharacterEncodings.WinAnsi,
        _ => CharacterEncodings.Standard
    };

    private void WriteCharacterSet(PdfDirectValue[] characters)
    {
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = nameMapper.GetGlyphFor(characters[i]);
        }
    }
    
    private async ValueTask WriteDifferencesAsync(PdfValueArray differences)
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
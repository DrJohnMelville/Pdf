using System.Diagnostics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

public readonly partial struct SingleByteEncodingParser
{
    [FromConstructor] private readonly INameToGlyphMapping nameMapper;
    [FromConstructor] private readonly uint[] output;
    [FromConstructor] private readonly byte[][]? overrideEncoding;

#if DEBUG
    partial void OnConstructed()
    {
        Debug.Assert(output.Length == 256);
    }
#endif

    public async ValueTask WriteEncodingToArray(PdfObject encoding)
    {
        switch (encoding)
        {
            case PdfName name:
                WriteCharacterSet(overrideEncoding ?? CharactersFromName(name));
                break;
            case PdfDictionary dict:
                await WriteEncodingToArray(await dict.GetOrNullAsync(KnownNames.BaseEncoding).CA()).CA();
                if ((await dict.GetOrNullAsync<PdfArray>(KnownNames.Differences).CA()) is {} differences )
                    await WriteDifferences(differences).CA();
                break; 
            default:
                WriteCharacterSet(overrideEncoding?? CharacterEncodings.Standard);
                break;
        }
    }
    
    private byte[][] CharactersFromName(PdfName name) => 
        name.GetHashCode() switch
    {
        KnownNameKeys.MacRomanEncoding => CharacterEncodings.MacRoman,
        KnownNameKeys.MacExpertEncoding => CharacterEncodings.MacExpert,
        KnownNameKeys.Symbol => CharacterEncodings.Symbol,
        KnownNameKeys.PdfDocEncoding => CharacterEncodings.Pdf,
        KnownNameKeys.WinAnsiEncoding => CharacterEncodings.WinAnsi,
        _ => CharacterEncodings.Standard
    };

    private void WriteCharacterSet(byte[][] characters)
    {
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = nameMapper.GetGlyphFor(characters[i]);
        }
    }
    
    private async ValueTask WriteDifferences(PdfArray differences)
    {
        byte currentChar = 0;
        await foreach (var item in differences.CA())
        {
            switch (item)
            {
                case PdfNumber num:
                    currentChar = (byte)num.IntValue;
                    break;
                case PdfName name:
                    var glyph = nameMapper.GetGlyphFor(name.Bytes);
                    if (glyph > 0) output[currentChar++] = glyph;
                    break;
            }
        }
    }

}
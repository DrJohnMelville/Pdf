using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.FontMappings;

namespace Melville.Pdf.Model.Renderers;

public readonly struct FontReader
{
    private readonly IDefaultFontMapper defaultMapper;

    public FontReader(IDefaultFontMapper defaultMapper)
    {
        this.defaultMapper = defaultMapper;
    }

    public async ValueTask<IFontMapping> DictionaryToMappingAsync(PdfDictionary font)
    {
        var baseFontName = await font.GetAsync<PdfName>(KnownNames.BaseFont);
        return (await font.GetOrDefaultAsync(KnownNames.Subtype, KnownNames.Type1)).GetHashCode() switch
        {
            KnownNameKeys.MMType1=> NameToMapping(RemoveMultMasterSuffix(baseFontName)),
            _ => NameToMapping(baseFontName)
        };
    }

    private PdfName RemoveMultMasterSuffix(PdfName baseFontName)
    {
        var source = baseFontName.Bytes.AsSpan();
        var firstUnderscore = source.IndexOf((byte)'_');
        return firstUnderscore < 0 ? baseFontName : NameDirectory.Get(source[..firstUnderscore]);
    }

    public IFontMapping NameToMapping(PdfName name) => defaultMapper.MapDefaultFont(name);
}
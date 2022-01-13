using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.FontMappings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

public readonly struct FontReader
{
    private readonly IDefaultFontMapper defaultMapper;

    public FontReader(IDefaultFontMapper defaultMapper)
    {
        this.defaultMapper = defaultMapper;
    }

    public async ValueTask<IFontMapping> DictionaryToMappingAsync(
        PdfDictionary font, IType3FontTarget target, double size)
    {
        if (font.TryGetValue(KnownNames.FontDescriptor, out var descTask) && 
            (await descTask) is PdfDictionary descriptor &&
            await StreamFromDescriptorAsync(descriptor) is { } fontAsStream)
            return new NamedDefaultMapping(fontAsStream, false, false);
        
        var baseFontName = await font.GetOrDefaultAsync(KnownNames.BaseFont, KnownNames.Helvetica);
        
        return (await font.GetOrDefaultAsync(KnownNames.Subtype, KnownNames.Type1)).GetHashCode() switch
        {
            KnownNameKeys.MMType1=> NameToMapping(RemoveMultMasterSuffix(baseFontName)),
            KnownNameKeys.Type3 => await new Type3FontFactory(font, target, size).ParseAsync(),
            _ => NameToMapping(baseFontName)
        };
    }

    private async ValueTask<PdfStream?> StreamFromDescriptorAsync(PdfDictionary descriptor) =>
        (descriptor.TryGetValue(KnownNames.FontFile2, out var ff2Task) ||
         descriptor.TryGetValue(KnownNames.FontFile3, out ff2Task)) 
       && (await ff2Task) is PdfStream ff2
            ? ff2
            : null;

    private PdfName RemoveMultMasterSuffix(PdfName baseFontName)
    {
        var source = baseFontName.Bytes.AsSpan();
        var firstUnderscore = source.IndexOf((byte)'_');
        return firstUnderscore < 0 ? baseFontName : NameDirectory.Get(source[..firstUnderscore]);
    }

    public IFontMapping NameToMapping(PdfName name) => defaultMapper.MapDefaultFont(name);
}
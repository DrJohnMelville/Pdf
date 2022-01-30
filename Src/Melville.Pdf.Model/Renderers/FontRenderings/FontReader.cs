using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterAndGlyphEncoding;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

public readonly struct FontReader
{
    private readonly IDefaultFontMapper defaultMapper;

    public FontReader(IDefaultFontMapper defaultMapper)
    {
        this.defaultMapper = defaultMapper;
    }

    public async ValueTask<IRealizedFont> DictionaryToRealizedFont(
        PdfDictionary dict, double size)
    {
        return await FontFromDictionaryAsync(dict, size).ConfigureAwait(false);
    }

    public ValueTask<IRealizedFont> NameToRealizedFont(PdfName name, double size) =>
        defaultMapper.MapDefaultFont(name, size);

    private async ValueTask<IRealizedFont> FontFromDictionaryAsync(
        PdfDictionary font, double size)
    {
        var fontTypeKey = 
            (await font.GetOrDefaultAsync(KnownNames.Subtype, KnownNames.Type1).ConfigureAwait(false))
            .GetHashCode();
        
        if (fontTypeKey == KnownNameKeys.Type3)
            return await new Type3FontFactory(font, size).ParseAsync().ConfigureAwait(false);

        PdfObject encoding = await font.GetOrNullAsync(KnownNames.Encoding).ConfigureAwait(false);
        PdfDictionary? descriptor = font.TryGetValue(KnownNames.FontDescriptor, out var descTask) ?
             (await descTask) as PdfDictionary: null;
        
        var ret = await CreateRealizedFont(font, size, descriptor, fontTypeKey).ConfigureAwait(false);
        await ret.SetGlyphEncoding(encoding, descriptor).ConfigureAwait(false);
        return ret;
    }

    private async Task<IRealizedFont> CreateRealizedFont(PdfDictionary font, double size, PdfDictionary? descriptor, int fontTypeKey) =>
        await (
                await StreamFromDescriptorAsync(descriptor).ConfigureAwait(false) is { } fontAsStream ?
                    FreeTypeFontFactory.FromStream(fontAsStream, size, null) :
                    SystemFontByName(font, size, fontTypeKey)
              ).ConfigureAwait(false);

    private async ValueTask<IRealizedFont> SystemFontByName(PdfDictionary font, double size, int fontTypeKey)
    {
        var baseFontName = await
            font.GetOrDefaultAsync(KnownNames.BaseFont, KnownNames.Helvetica).ConfigureAwait(false);

        return await defaultMapper.MapDefaultFont(ComputeOsFontName(fontTypeKey, baseFontName), size)
            .ConfigureAwait(false);
    }

    private PdfName ComputeOsFontName(int fontType, PdfName baseFontName) =>
        fontType == KnownNameKeys.MMType1?
            RemoveMultMasterSuffix(baseFontName):baseFontName;

    private async ValueTask<PdfStream?> StreamFromDescriptorAsync(PdfDictionary? descriptor) =>
        descriptor != null && 
        (descriptor.TryGetValue(KnownNames.FontFile2, out var ff2Task) ||
         descriptor.TryGetValue(KnownNames.FontFile3, out ff2Task)) 
       && (await ff2Task.ConfigureAwait(false)) is PdfStream ff2
            ? ff2
            : null;

    private PdfName RemoveMultMasterSuffix(PdfName baseFontName)
    {
        var source = baseFontName.Bytes.AsSpan();
        var firstUnderscore = source.IndexOf((byte)'_');
        return firstUnderscore < 0 ? baseFontName : NameDirectory.Get(source[..firstUnderscore]);
    }
}
using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
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
    
    public ValueTask<IRealizedFont> NameToRealizedFont(PdfName name, FreeTypeFontFactory factory) =>
        defaultMapper.MapDefaultFont(name, factory);

    public async ValueTask<IRealizedFont> DictionaryToRealizedFont(PdfDictionary font, double size)
    {
        var fontTypeKey = 
            (await font.GetOrDefaultAsync(KnownNames.Subtype, KnownNames.Type1).CA())
            .GetHashCode();
        
        if (fontTypeKey == KnownNameKeys.Type3)
            return await new Type3FontFactory(font, size).ParseAsync().CA();

        PdfObject encoding = await font.GetOrNullAsync(KnownNames.Encoding).CA();
        PdfDictionary? descriptor = font.TryGetValue(KnownNames.FontDescriptor, out var descTask) ?
             (await descTask) as PdfDictionary: null;

        var fontFactory = new FreeTypeFontFactory(size, null, null);
        var ret = await CreateRealizedFont(font, fontFactory, descriptor, fontTypeKey).CA();
        #warning setGlyphEncoding shoud move into the freetypefontfactory
        await ret.SetGlyphEncoding(encoding, descriptor).CA();
        return ret;
    }

    private async Task<IRealizedFont> CreateRealizedFont(PdfDictionary font, FreeTypeFontFactory factory, PdfDictionary? descriptor, int fontTypeKey) =>
        await (
                await StreamFromDescriptorAsync(descriptor).CA() is { } fontAsStream ?
                    factory.FromStream(fontAsStream) :
                    SystemFontByName(font, factory, fontTypeKey)
              ).CA();

    private async ValueTask<IRealizedFont> SystemFontByName(PdfDictionary font, FreeTypeFontFactory factory, int fontTypeKey)
    {
        var baseFontName = await
            font.GetOrDefaultAsync(KnownNames.BaseFont, KnownNames.Helvetica).CA();

        return await defaultMapper.MapDefaultFont(ComputeOsFontName(fontTypeKey, baseFontName), factory)
            .CA();
    }

    private PdfName ComputeOsFontName(int fontType, PdfName baseFontName) =>
        fontType == KnownNameKeys.MMType1?
            RemoveMultMasterSuffix(baseFontName):baseFontName;

    private async ValueTask<PdfStream?> StreamFromDescriptorAsync(PdfDictionary? descriptor) =>
        descriptor != null && 
        (descriptor.TryGetValue(KnownNames.FontFile2, out var ff2Task) ||
         descriptor.TryGetValue(KnownNames.FontFile3, out ff2Task)) 
       && (await ff2Task.CA()) is PdfStream ff2
            ? ff2
            : null;

    private PdfName RemoveMultMasterSuffix(PdfName baseFontName)
    {
        var source = baseFontName.Bytes.AsSpan();
        var firstUnderscore = source.IndexOf((byte)'_');
        return firstUnderscore < 0 ? baseFontName : NameDirectory.Get(source[..firstUnderscore]);
    }
}
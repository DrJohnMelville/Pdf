using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
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

    public async ValueTask<IRealizedFont> DictionaryToRealizedFont(PdfDictionary fontDict, double size)
    {
        var font = new PdfFont(fontDict);
        var fontTypeKey = (await font.SubTypeAsync().CA()).GetHashCode();
        
        if (fontTypeKey == KnownNameKeys.Type3)
            return await new Type3FontFactory(font.LowLevel, size).ParseAsync().CA();

        var encoding = await font.EncodingAsync().CA();
        PdfDictionary? descriptor = await font.DescriptorAsync().CA();

        var fontFactory = new FreeTypeFontFactory(size, null, font);
        var ret = await CreateRealizedFont(font, fontFactory).CA();
        return ret;
    }

    private async Task<IRealizedFont> CreateRealizedFont(PdfFont font, FreeTypeFontFactory factory) =>
        await (
                await font.EmbeddedStreamAsync().CA() is { } fontAsStream ?
                    factory.FromStream(fontAsStream) :
                    SystemFontByName(font, factory)
              ).CA();

    private async ValueTask<IRealizedFont> SystemFontByName(PdfFont font, FreeTypeFontFactory factory) =>
        await defaultMapper.MapDefaultFont(await font.OsFontNameAsync().CA(), factory)
            .CA();

}
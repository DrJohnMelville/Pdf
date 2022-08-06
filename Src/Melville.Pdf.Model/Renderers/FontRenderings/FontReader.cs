using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

public readonly struct FontReader
{
    private readonly IDefaultFontMapper defaultMapper;

    public FontReader(IDefaultFontMapper defaultMapper)
    {
        this.defaultMapper = defaultMapper;
    }
    
    public ValueTask<IRealizedFont> NameToRealizedFont(PdfName name, FreeTypeFontFactory factory) =>
        defaultMapper.MapDefaultFont(name, FontFlags.None, factory);

    public  ValueTask<IRealizedFont> DictionaryToRealizedFont(PdfDictionary fontDict, double size) => 
         PdfFontToRealizedFont(size, new PdfFont(fontDict));

    private async ValueTask<IRealizedFont> PdfFontToRealizedFont(double size, PdfFont font)
    {
        var fontTypeKey = (await font.SubTypeAsync().CA()).GetHashCode();
        return fontTypeKey switch
        {
            KnownNameKeys.Type3 => await new Type3FontFactory(font.LowLevel, size).ParseAsync().CA(),
            KnownNameKeys.Type0 => await CreateType0Font(font, size).CA(),
            _ => await CreateRealizedFont(font, new FreeTypeFontFactory(size, font)).CA()
        };
    }


    private async ValueTask<IRealizedFont> CreateRealizedFont(PdfFont font, FreeTypeFontFactory factory) =>
        await (
                await font.EmbeddedStreamAsync().CA() is { } fontAsStream ?
                    factory.FromStream(fontAsStream) :
                    SystemFontByName(font, factory)
              ).CA();

    private async ValueTask<IRealizedFont> SystemFontByName(PdfFont font, FreeTypeFontFactory factory) =>
        await defaultMapper.MapDefaultFont(await font.OsFontNameAsync().CA(), await font.FontFlagsAsync().CA(), factory)
            .CA();

    private async ValueTask<IRealizedFont> CreateType0Font(PdfFont font, double size)
    {
        var sub = await font.Type0SubFont().CA();
        var mapper =
            await ParseCidToGidMap(await sub.CidToGidMapStream().CA(),
                  await ParseType0Encoding(await font.EncodingAsync().CA()).CA()).CA();
        return await PdfFontToRealizedFont(size, sub).CA();
    }

    private ValueTask<IGlyphMapping> ParseCidToGidMap(PdfStream? mapStream, IGlyphMapping innerMapping) => 
        mapStream is null ? new(innerMapping) : ExplicitMappingFactory.Parse(innerMapping, mapStream);

    private ValueTask<IGlyphMapping> ParseType0Encoding(PdfObject? encodingEntry)
    {
        if (encodingEntry != KnownNames.IdentityH && encodingEntry != KnownNames.IdentityV)
            throw new NotImplementedException("Cmaps (section 9.7.5.1) other than Identity are not yet implemented.");
        return new (IdentityCmapMapping.Instance);
    }
}
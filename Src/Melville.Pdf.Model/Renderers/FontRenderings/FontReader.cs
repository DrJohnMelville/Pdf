using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.CSJ2K.j2k.wavelet;
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
        defaultMapper.FontFromName(name, FontFlags.None, factory);

    public  ValueTask<IRealizedFont> DictionaryToRealizedFont(PdfDictionary fontDict, double size) => 
         PdfFontToRealizedFont(size, new PdfFont(fontDict));

    private async ValueTask<IRealizedFont> PdfFontToRealizedFont(double size, PdfFont font)
    {
        var fontTypeKey = (await font.SubTypeAsync().CA()).GetHashCode();
        return fontTypeKey switch
        {
            KnownNameKeys.Type3 => await new Type3FontFactory(font.LowLevel, size).ParseAsync().CA(),
            KnownNameKeys.Type0 => await CreateType0Font(font, new FreeTypeFontFactory(size, font)).CA(),
            _ => await CreateRealizedFont(font, new FreeTypeFontFactory(size, font)).CA()
        };
    }


    private async ValueTask<IRealizedFont> CreateType0Font(PdfFont font, FreeTypeFontFactory factory)
    {
        Debug.Assert(KnownNames.Type0 == await font.SubTypeAsync().CA());
        var cidFont = await font.Type0SubFont().CA();
        return await CreateRealizedFont(cidFont, factory).CA();
    }
      
    private async ValueTask<IRealizedFont> CreateRealizedFont(PdfFont fontStreamSource, FreeTypeFontFactory factory) =>
        // notice that when parsing a type 0 font the font reference in the factory may be different
        // from the FontStreamSource paramenter.
        await (
                await fontStreamSource.EmbeddedStreamAsync().CA() is { } fontAsStream ?
                    factory.FromStream(fontAsStream) :
                    SystemFontByName(fontStreamSource, factory)
              ).CA();

    private async ValueTask<IRealizedFont> SystemFontByName(PdfFont font, FreeTypeFontFactory factory) =>
        await defaultMapper.FontFromName(await font.OsFontNameAsync().CA(), await font.FontFlagsAsync().CA(), factory)
            .CA();
}
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

/// <summary>
/// Reads font files from a stream into an IRealizedFont
/// </summary>
public readonly struct FontReader
{
    private readonly IDefaultFontMapper defaultMapper;

    /// <summary>
    /// Create a FontReader
    /// </summary>
    /// <param name="defaultMapper">IDefaultFontMapper to map the builting fonts to real fonts</param>
    public FontReader(IDefaultFontMapper defaultMapper)
    {
        this.defaultMapper = defaultMapper;
    }
    
    /// <summary>
    /// Gets an IRealizedFont from a Pdf font dictionary
    /// </summary>
    /// <param name="fontDict">A PdfDictionary representing the font</param>
    /// <returns>An IRealizedFont that can render characters in the font.</returns>
    public  ValueTask<IRealizedFont> DictionaryToRealizedFontAsync(PdfDictionary fontDict) => 
         PdfFontToRealizedFontAsync(new PdfFont(fontDict));

    private ValueTask<IRealizedFont> PdfFontToRealizedFontAsync(PdfFont font)
    {
        var fontTypeKey = font.SubType();
        return fontTypeKey switch
        {
            var x when x.Equals(KnownNames.Type3TName) => new Type3FontFactory(font.LowLevel).ParseAsync(),
            var x when x.Equals(KnownNames.Type0TName) => CreateType0FontAsync(font, new FreeTypeFontFactory(font)),
            _ => CreateRealizedFontAsync(font, new FreeTypeFontFactory(font))
        };
    }


    private async ValueTask<IRealizedFont> CreateType0FontAsync(PdfFont font, FreeTypeFontFactory factory)
    {
        Debug.Assert(KnownNames.Type0TName.Equals(font.SubType()));
        var cidFont = await font.Type0SubFontAsync().CA();
        return await CreateRealizedFontAsync(cidFont, factory).CA();
    }
      
    private async ValueTask<IRealizedFont> CreateRealizedFontAsync(PdfFont fontStreamSource, FreeTypeFontFactory factory) =>
        // notice that when parsing a type 0 font the font reference in the factory may be different
        // from the FontStreamSource paramenter.
        await (
                await fontStreamSource.EmbeddedStreamAsync().CA() is { } fontAsStream ?
                    factory.FromStreamAsync(fontAsStream) :
                    SystemFontByNameAsync(fontStreamSource, factory)
              ).CA();

    private async ValueTask<IRealizedFont> SystemFontByNameAsync(PdfFont font, FreeTypeFontFactory factory) =>
        await defaultMapper
            .FontFromName(await font.OsFontNameAsync().CA(), await font.FontFlagsAsync().CA())
            .ToFontAsync(factory).CA();
}

internal static class MapDefaultFontReferenceToFont
{
    public static ValueTask<IRealizedFont> ToFontAsync(this DefaultFontReference reference, FreeTypeFontFactory factory) =>
        factory.FromCSharpStreamAsync(reference.Source, reference.Index);
}
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.FontLibrary.Cjk;

/// <summary>
/// This is an IDefaultFontMapper that maps all CJK fonts to a set of fonts contained as
/// resources in this assembly.  For Latin fonts this class defers to another DefaultFontMapper
/// The instance class uses SelfContainedDefaultFonts to map the Latin fonts.
/// </summary>
public class SelfContainedCjkFonts(IDefaultFontMapper latinFonts): IDefaultFontMapper
{
    /// <summary>
    /// The single instance of the SelfContainedCjkFonts instance
    /// </summary>
    public static readonly IDefaultFontMapper Instance =
        new SelfContainedCjkFonts(SelfContainedDefaultFonts.Instance);

    /// <inheritdoc />
    public async ValueTask<DefaultFontReference> FontReferenceForAsync(PdfFont font)
    {
        var flags = await font.FontFlagsAsync();
        var ordering = await font.FontAsianLanguageAsync();

        if (ordering is { } asianLang)
        {
            return await SystemFontAsync(flags.HasFlag(FontFlags.Serif) ?
                "NotoSerifCJK-VF.otf.ttc": "NotoSansCJK-VF.otf.ttc", (int)ordering);
        }

        return await latinFonts.FontReferenceForAsync(font);
    }

    private ValueTask<DefaultFontReference> SystemFontAsync(string fileName, int ordering) =>
        new(new DefaultFontReference(
            FontStream(fileName) ??
            throw new InvalidOperationException("Cannot find font resource: " + fileName), ordering));

    private Stream? FontStream(string fileName) =>
        GetType().Assembly.GetManifestResourceStream("Melville.Pdf.FontLibrary.Cjk." + fileName);


    /// <inheritdoc />
    public ValueTask<DefaultFontReference> FontFromNameAsync(PdfDirectObject font, FontFlags flags) =>
        FontReferenceForAsync(new (new DictionaryBuilder()
            .WithItem(KnownNames.BaseFont, font)
            .WithItem(KnownNames.FontDescriptor, new DictionaryBuilder()
                .WithItem(KnownNames.Flags, (uint)flags).AsDictionary()).AsDictionary()));
}
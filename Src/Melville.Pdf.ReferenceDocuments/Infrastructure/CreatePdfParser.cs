using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.FontLibrary;
using Melville.Pdf.Model;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.ReferenceDocuments.Infrastructure;

public abstract class CreatePdfParser : IPdfGenerator
{
    public string Prefix { get; }
    public string HelpText { get; }
    public virtual string Password => "";

    protected CreatePdfParser(string helpText)
    {
        Prefix = "-"+this.GetType().Name;
        HelpText = helpText;
    }

    public abstract ValueTask WritePdfAsync(Stream target);
}

public static class CreatePdfParserOperations
{
    public static async ValueTask<IMultiplexSource> AsMultiBufAsync(this CreatePdfParser source)
    {
        var target = WritableBuffer.Create();
        await using var writer = target.WritingStream();
        await source.WritePdfAsync(writer);
        return target;
    }

    public static async ValueTask<string> AsStringAsync(this CreatePdfParser source) =>
        await new StreamReader((await source.AsMultiBufAsync()).ReadFrom(0))
            .ReadToEndAsync();

    public static async ValueTask<DocumentRenderer> AsDocumentRendererAsync(
        this CreatePdfParser source, IDefaultFontMapper? fonts = null) =>
        await new PdfReader(fonts?? SelfContainedDefaultFonts.Instance).ReadFromAsync(await source.AsMultiBufAsync());
}
using Melville.Parsing.Streams;

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
    public static async ValueTask<MultiBufferStream> AsMultiBufAsync(this CreatePdfParser source)
    {
        var target = new MultiBufferStream();
        await source.WritePdfAsync(target);
        return target;
    }

    public static async ValueTask<string> AsStringAsync(this CreatePdfParser source) =>
        await new StreamReader((await source.AsMultiBufAsync()).CreateReader()).ReadToEndAsync();
}
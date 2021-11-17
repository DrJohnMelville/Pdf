namespace Melville.Pdf.ReferenceDocuments.Infrastructure;

public abstract class CreatePdfParser : IPdfGenerator
{
    public string Prefix { get; }
    public string HelpText { get; }

    protected CreatePdfParser(string helpText)
    {
        Prefix = "-"+this.GetType().Name;
        HelpText = helpText;
    }

    public abstract ValueTask WritePdfAsync(Stream target);
}

public static class CreatePdfParserOperations
{
    public static async ValueTask<MultiBufferStream> AsMultiBuf(this CreatePdfParser source)
    {
        var target = new MultiBufferStream();
        await source.WritePdfAsync(target);
        return target;
    }

    public static async ValueTask<string> AsString(this CreatePdfParser source) =>
        await new StreamReader((await source.AsMultiBuf()).CreateReader()).ReadToEndAsync();
}
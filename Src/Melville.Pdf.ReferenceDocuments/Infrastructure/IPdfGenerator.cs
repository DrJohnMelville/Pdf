using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model;

namespace Melville.Pdf.ReferenceDocuments.Infrastructure;

public interface IPdfGenerator
{
    public string Prefix { get; }
    public string HelpText { get; }
    public string Password { get; }
    public ValueTask WritePdfAsync(Stream target);
}


public static class RenderTestHelpers
{
    public static async ValueTask<DocumentRenderer> ReadDocumentAsync(this IPdfGenerator generator)
    {
        using var src = WritableBuffer.Create();
        await using var writer = src.WritingStream();
        await generator.WritePdfAsync(writer);
        await using var readFrom = src.ReadFrom(0);
        return await new PdfReader(new ConstantPasswordSource(PasswordType.User, generator.Password))
            .ReadFromAsync(readFrom);
    }
}
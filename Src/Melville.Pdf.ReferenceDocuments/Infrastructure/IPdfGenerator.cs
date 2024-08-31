using System.Diagnostics;
using Melville.Hacks.Reflection;
using Melville.Parsing.CountingReaders;
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
        var src = WritableBuffer.Create();
        await using var writer = src.WritingStream();
        await generator.WritePdfAsync(writer);
        return await new PdfReader(new ConstantPasswordSource(PasswordType.User, generator.Password))
            .ReadFromAsync((src));
    }
}

internal class DebugIntermediary(IMultiplexSource inner) : IMultiplexSource
{
    private List<ReaderFor> readers = new List<ReaderFor>();

    private T Record<T>(T item)
    {
        readers.Add(new ReaderFor(item!));
        return item;
    }

    public Stream ReadFrom(long position)
    {
        return Record(inner.ReadFrom(position));
    }

    public IByteSource ReadPipeFrom(long position, long startingPosition = 0)
    {
        return Record(inner.ReadPipeFrom(position, startingPosition));
    }

    public long Length => inner.Length;

    public IMultiplexSource OffsetFrom(uint offset)
    {
        return Record(inner.OffsetFrom(offset));
    }

    public void Dispose()
    {
        inner.Dispose();
    }
}

public record ReaderFor(string StackTrace, object reader)
{
    public ReaderFor(object reader): this(new StackTrace().ToString(), reader){}
}
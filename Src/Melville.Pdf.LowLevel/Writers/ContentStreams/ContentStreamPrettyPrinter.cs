using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.StreamInterfaces;

namespace Melville.Pdf.LowLevel.Writers.ContentStreams;

/// <summary>
/// Formats a content stream with 1 operation per line and indenting.
/// </summary>
public static class ContentStreamPrettyPrinter
{
    /// <summary>
    /// Format a string representing a valid content stream with the pretty printer.
    /// </summary>
    /// <param name="content">The content stream to format.</param>
    /// <returns>A pretty printed version of content.</returns>
    public static async ValueTask<string> PrettyPrintAsync(string content)
    {
        MemoryStream dest = new();
        var pipeWriter = PipeWriter.Create(dest);
        var parser = new ContentStreamParser(new IndentingContentStreamWriter(pipeWriter));
        using var multiplexSource = MultiplexSourceFactory.Create(content.AsExtendedAsciiBytes());
        using var _ = Force.Using(multiplexSource.ReaderFrom(0), out var readPipe);
        await parser.ParseAsync(readPipe).CA();
        await pipeWriter.FlushAsync().CA();
        return new ReadOnlySpan<byte>(dest.GetBuffer(), 0, (int)dest.Length).ExtendedAsciiString();
    }
}
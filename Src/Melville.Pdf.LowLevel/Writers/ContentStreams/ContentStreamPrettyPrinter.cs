using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;

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
    public static async ValueTask<string> PrettyPrint(string content)
    {
        MemoryStream dest = new();
        var pipeWriter = PipeWriter.Create(dest);
        var parser = new ContentStreamParser(new IndentingContentStreamWriter(pipeWriter));
        await parser.Parse(PipeReader.Create(new ReadOnlySequence<byte>(content.AsExtendedAsciiBytes()))).CA();
        await pipeWriter.FlushAsync().CA();
        return new ReadOnlySpan<byte>(dest.GetBuffer(), 0, (int)dest.Length).ExtendedAsciiString();
    }
}
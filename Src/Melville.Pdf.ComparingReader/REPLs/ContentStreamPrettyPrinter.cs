using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ComparingReader.REPLs;

public static class ContentStreamPrettyPrinter
{
    public static async ValueTask<string> PrettyPrint(string content)
    {
        MemoryStream dest = new();
        var pipeWriter = PipeWriter.Create(dest);
        var parser = new ContentStreamParser(new IndentingContentStreamWriter(pipeWriter));
        await parser.Parse(PipeReader.Create(new ReadOnlySequence<byte>(content.AsExtendedAsciiBytes())));
        await pipeWriter.FlushAsync();
        return new ReadOnlySpan<byte>(dest.GetBuffer(), 0, (int)dest.Length).ExtendedAsciiString();
    }
}
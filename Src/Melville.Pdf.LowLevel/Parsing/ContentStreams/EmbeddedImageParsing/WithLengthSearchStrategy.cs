using Melville.INPC;
using Melville.StreamInterfaces.ParsingBuffers;
using System.Buffers;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

internal partial class WithLengthSearchStrategy: EndSearchStrategy
{
    [FromConstructor] private readonly int length;

    protected override bool SkipBytes(ref SpanParsingBuffer seqReader)
    {
        if (!base.SkipBytes(ref seqReader)) return false;
        if (seqReader.CurrentLength< length) return false;
        seqReader.Advance(length);
        return true;
    }
} 
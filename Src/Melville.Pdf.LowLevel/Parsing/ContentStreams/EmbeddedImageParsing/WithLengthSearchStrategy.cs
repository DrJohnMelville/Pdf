using System.Buffers;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

internal partial class WithLengthSearchStrategy: EndSearchStrategy
{
    [FromConstructor] private readonly int length;

    protected override bool SkipBytes(ref SequenceReader<byte> seqReader)
    {
        if (!base.SkipBytes(ref seqReader)) return false;
        if (seqReader.Remaining < length) return false;
        seqReader.Advance(length);
        return true;
    }
}
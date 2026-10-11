using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.StreamInterfaces.ParsingBuffers;
using System.Buffers;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

[FromConstructor]
internal partial class WhiteSpaceAndLengthSearchStrategy : WithLengthSearchStrategy
{
    protected override bool SkipBytes(ref SpanParsingBuffer seqReader)
    {
        // the assertion is a deconstructed implication.
        Debug.Assert(!seqReader.TryPeekUInt8(out var assVar) || CharClassifier.IsWhite(assVar));

        while (seqReader.TryPeekUInt8(out var value) && CharClassifier.IsWhite(value)) ;
        return base.SkipBytes(ref seqReader);
    }
}
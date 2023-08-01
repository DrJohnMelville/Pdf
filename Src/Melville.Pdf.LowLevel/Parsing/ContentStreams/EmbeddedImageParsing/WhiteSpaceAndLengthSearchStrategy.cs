using System.Buffers;
using System.Diagnostics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

[FromConstructor]
internal partial class WhiteSpaceAndLengthSearchStrategy : WithLengthSearchStrategy
{
    protected override bool SkipBytes(ref SequenceReader<byte> seqReader)
    {
        // the assertion is a deconstructed implication.
        Debug.Assert(!seqReader.TryPeek(out var assVar) || CharClassifier.IsWhite(assVar));

        while (seqReader.TryPeek(1, out var value) && CharClassifier.IsWhite(value)) ;
        return base.SkipBytes(ref seqReader);
    }
}
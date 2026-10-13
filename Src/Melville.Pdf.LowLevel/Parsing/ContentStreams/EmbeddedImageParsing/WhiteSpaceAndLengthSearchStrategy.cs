using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

[FromConstructor]
internal partial class WhiteSpaceAndLengthSearchStrategy : WithLengthSearchStrategy
{
    protected override bool SkipBytes(ref SpanParsingBuffer seqReader)
    {
        // the assertion is a deconstructed implication.
        Debug.Assert(!seqReader.TryPeekUInt8(out var assVar) || 
            CharacterClassifier.IsWhitespace(assVar));

        var index = seqReader.Peek().IndexOfAnyExcept(CharacterClassifier.WhitespaceSearchValues);
        if (index < 0) return false;
        seqReader.Advance(index);
        return base.SkipBytes(ref seqReader);
    }
}
using System;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Filters.Ascii85Filter;

internal class Ascii85Decoder : StringDecoderStreamFilterDefinition<Ascii85StringDecoder, byte>
{
    public override int MinWriteSize => 4;

    protected override ReadOnlySpan<byte> TerminatorSequence() => "~>"u8;

}
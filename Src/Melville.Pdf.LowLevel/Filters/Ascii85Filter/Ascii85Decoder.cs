using Melville.Parsing.SequenceReaders;
using Melville.Parsing.StreamFilters;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Melville.Pdf.LowLevel.Filters.Ascii85Filter;


internal class Ascii85Decoder() : IStreamFilterDefinition
{
    private (uint,int,int) memento;
   

    public bool Convert(ref SpanParsingBuffer input, ref Span<byte> output) =>
        InnerConvert(ref input, ref output, false);

    private bool InnerConvert(ref SpanParsingBuffer input, ref Span<byte> output, bool final)
    {
        var engine = new Ascii85DecoderCore(memento);
        var ret = engine.InnerConvert(ref input, ref output, final);
        memento = engine.GetMemento();
        return ret;
    }

    public bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output) =>
        InnerConvert(ref input, ref output, true);

    /// <summary>
    /// Minimum size of a buffer that can be written to.
    /// </summary>
    public int MinWriteSize => 0;
}


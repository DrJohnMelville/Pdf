using System;
using System.Buffers;
using System.Collections.Generic;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal static class SynchronousTokenizer
{
    public static IEnumerable<PostscriptValue> Tokenize(Memory<byte> input)
    {
        var seq = input.AppendCR();
        while (ReadFrom(ref seq, out var token))
            yield return token;
    }

    public static bool ReadFrom(ref ReadOnlySequence<byte> seq, out PostscriptValue token)
    {
        var reader = new SequenceReader<byte>(seq);
        var ret = reader.TryGetPostscriptToken(out token);
        seq = seq.Slice(reader.Consumed);
        return ret;
    }
}
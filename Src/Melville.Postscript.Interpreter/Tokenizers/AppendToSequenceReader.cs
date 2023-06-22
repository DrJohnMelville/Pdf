using System;
using System.Buffers;

namespace Melville.Postscript.Interpreter.Tokenizers
{
    internal static class AppendToSequenceReader
    {
        public static ReadOnlySequence<T> Append<T>(this ReadOnlySequence<T> seq, Memory<T> mem)
        {
            var builder = new ReadOnlySequenceBuilder<T>();
            builder.Append(seq);
            builder.Append(mem);
            return builder.GetSequence();
        }

        private static readonly byte[] terminalWhitespace = { (byte)'\r' };

        public static ReadOnlySequence<byte> AppendCR(this ReadOnlySequence<byte> seq) => 
            seq.Append(terminalWhitespace);

        public static ReadOnlySequence<byte> AppendCR(this Memory<byte> mem)
        {
            var builder = new ReadOnlySequenceBuilder<byte>();
            builder.Append(mem);
            builder.Append(terminalWhitespace);
            return builder.GetSequence();

        }
    }
}
using System;
using System.Buffers;

namespace Melville.Postscript.Interpreter.Tokenizers
{
    internal static class SequenceReaderExtension
    {
        public static ref SequenceReader<T> WithAdvance<T>(this ref SequenceReader<T> sequence, int count = 1) where T : unmanaged, IEquatable<T>
        {
            sequence.Advance(count);
            return ref sequence;
        }
    }
}
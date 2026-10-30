using System.Buffers;
using System.Linq;

namespace Melville.Postscript.Interpreter.Tokenizers
{
    /// <summary>
    /// Constants for decoding Ascii84
    /// </summary>
    public static class Ascii85Constants
    {
        /// <summary>
        /// First character of an ascii85 stream
        /// </summary>
        public const byte FirstChar = (byte)'!';
        /// <summary>
        /// Byte used to pad incomplete groups
        /// </summary>
        public const byte IncompleteGroupPadding = (byte)'u';
        /// <summary>
        /// The first character of the terminating sequence
        /// </summary>
        public const byte FirstTerminatingChar = (byte)'~';
        /// <summary>
        /// Second character of the terminating sequence
        /// </summary>
        public const byte SecondTerminatingChar = (byte)'>';
        /// <summary>
        /// code for 4 consectuve zeros
        /// </summary>
        public const byte FourZeroChar = (byte)'z';

//        public static readonly SearchValues<byte> Valid = SearchValues.Create(
  //          Enumerable.Sequence((byte)33, (byte)117, (byte)1).ToArray());
    }
}
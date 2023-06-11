using System;

namespace Melville.Postscript.Interpreter.Values.Strings
{
    /// <summary>
    /// This represents a PostScriptValue that is a span on bytes -- that is names or strings
    /// </summary>
    internal interface IByteStringSource
    {
        /// <summary>
        /// Fetch at least scratch.Length bytes of the enclosed byte string.  The method
        /// may, but is not required to use the scratch span as storage for the returned span.
        /// The method may, but is not required to return more characters than would fit in the
        /// supplied scratch span.  In the current implementation calling this method with a span
        /// with length greater than or equal to ShortStringLimit will always return the whole string.
        /// </summary>
        /// <param name="memento">The memento from the PostscriptValue</param>
        /// <param name="scratch">A scratch space that can, but does not have to contain the return</param>
        /// <returns>A span with the encoded bytes -- as noted above.</returns>
        ReadOnlySpan<byte> GetBytes(in Int128 memento, in Span<byte> scratch);

        /// <summary>
        /// The longest string which can be packed into an PostscriptValue.  Strings longer than this
        /// will be stored on the heap and the heap buffer returned from GetBytes.
        /// </summary>
        public const int ShortStringLimit = 18;
    }
}
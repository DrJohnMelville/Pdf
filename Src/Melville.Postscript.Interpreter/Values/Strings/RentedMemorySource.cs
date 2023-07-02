using System;
using System.Buffers;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values
{
    /// <summary>
    /// This structure is used to get a memory from a Postscript string (mainly so that rendering
    /// content streams can be allocation free for short strings.  If the memory is backed by a
    /// rented buffer than this is returned in the dispose method.
    /// </summary>
    public readonly partial struct RentedMemorySource : IDisposable
    {
        /// <summary>
        /// The memory that is the data value.
        /// </summary>
        [FromConstructor] public Memory<byte> Memory { get; }
        /// <summary>
        /// If the above memory is backed by a rented array it is stored here.
        /// </summary>
        [FromConstructor] private readonly byte[]? rentedArray;

        public void Dispose()
        {
            if (rentedArray is null) return;
            ArrayPool<byte>.Shared.Return(rentedArray);
        }
    }
}
using System;

namespace Melville.Postscript.Interpreter.InterpreterState
{
    /// <summary>
    /// The postscript spec requires a random number generator which has 32 bits of state that can
    /// be set or read by language operations.  The Lehmer PRNG is a good enough algorithm that
    /// satisfies the above criteria.  To be clear this PRNG has some limitations but it ought to
    /// be good enough for generating PDF documents.
    /// </summary>
    public struct LehmerRandomNumberGenerator
    {
        /// <summary>
        /// Create a random number generator with a seed derived from the system PRNG
        /// </summary>
        public LehmerRandomNumberGenerator() => State = 
            (uint)Random.Shared.Next(int.MaxValue - 3) + 1;
        /// <summary>
        /// The 32 bit state of the PRNG, which happens to also be the last number generated.
        /// </summary>
        public uint State { get; set; }
        /// <summary>
        /// Generate, store, and return the next pseudo random number generator;
        /// </summary>
        /// <returns>A random number.</returns>
        public uint Next() => State = (uint)( ((ulong)State * 48271) % 0x7FFFFFFF);
    }
}
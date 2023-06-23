using System;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values
{
    internal partial struct StringSpanSource
    {
        [FromConstructor] private PostscriptString strategy;
        [FromConstructor] private Int128 memento;

        public Span<byte> GetSpan(Span<byte> scratch) =>
            strategy.GetBytes(memento, scratch);
    }
}
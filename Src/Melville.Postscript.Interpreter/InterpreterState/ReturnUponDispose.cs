using System;
using System.Buffers;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.InterpreterState
{
    internal readonly partial struct ReturnUponDispose<T> : IDisposable
    {
        [FromConstructor] private readonly T[] array;

        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(array);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melville.Postscript.Interpreter.InterpreterState
{
    public readonly partial struct HybridEnumerator<T>
    {
        public object InnerEnumerator { get; }
        public HybridEnumerator(IAsyncEnumerator<T> enumerator) => InnerEnumerator = enumerator;
        public HybridEnumerator(IEnumerator<T> enumerator) => InnerEnumerator = enumerator;

        public T Current => InnerEnumerator switch
        {
            IAsyncEnumerator<T> ae => ae.Current,
            IEnumerator<T> e => e.Current,
            _ => throw new ArgumentOutOfRangeException()
        };

        public ValueTask<bool> MoveNextAsync() => InnerEnumerator switch
        {
            IAsyncEnumerator<T> ae => ae.MoveNextAsync(),
            IEnumerator<T> e => new(e.MoveNext()),
            _ => throw new ArgumentOutOfRangeException()
        };
    
        public bool MoveNext() => InnerEnumerator switch
        {
            IAsyncEnumerator<T> ae => throw new InvalidOperationException("Cannot synchronously read async enumerator"),
            IEnumerator<T> e => e.MoveNext(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
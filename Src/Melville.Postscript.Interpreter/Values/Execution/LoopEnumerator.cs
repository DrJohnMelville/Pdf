using Melville.INPC;
using System.Collections;
using System.Collections.Generic;

namespace Melville.Postscript.Interpreter.Values.Execution;

internal partial class LoopEnumerator : IEnumerator<PostscriptValue>
{
    [FromConstructor]
    [DelegateTo()]
    private readonly IEnumerator<PostscriptValue> innerEnumerator;
    
    public PostscriptValue Current => this.innerEnumerator.Current;

    object IEnumerator.Current => this.innerEnumerator.Current;
}
using Melville.INPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melville.Postscript.Interpreter.Values.Execution;

internal partial class LoopEnumerator : IAsyncEnumerator<PostscriptValue>
{
    [FromConstructor]
    [DelegateTo]
    private readonly IAsyncEnumerator<PostscriptValue> innerEnumerator;
}
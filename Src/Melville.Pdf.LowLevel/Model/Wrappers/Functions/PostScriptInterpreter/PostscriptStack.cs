using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

internal class PostscriptStack : List<double>
{
    public void Push(double item) => Add(item);

    public double Pop()
    {
        var last = this.Count - 1;
        var ret = this[last];
        RemoveAt(last);
        return ret;
    }

    public double Peek() => this[Count - 1];

    public Span<double> AsSpan() => CollectionsMarshal.AsSpan(this);

    public void Exchange()
    {
        var span = AsSpan();
        (span[^2], span[^1]) = (span[^1], span[^2]);
    }

    public double Peek(int item) => AsSpan()[^(item + 1)];
}
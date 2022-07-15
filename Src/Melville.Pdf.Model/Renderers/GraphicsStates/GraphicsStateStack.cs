using System;
using System.Collections.Generic;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

public sealed partial class GraphicsStateStack<T> : IGraphicsState, IDisposable
    where T: GraphicsState, new()
{ 
    private readonly Stack<T> states;
    public T Current() => states.Peek();
   
    public GraphicsStateStack()
    {
        states = new ();
        states.Push(new T());
    }

    public void SaveGraphicsState()
    {
        var newTop = new T();
        newTop.CopyFrom(Current());
        states.Push(newTop);
    }

    public void RestoreGraphicsState()
    {
        Current().Dispose();
        states.Pop();
    }

    [DelegateTo]
    private IGraphicsState topState => states.Peek();

    public void Dispose()
    {
        while (states.Count > 0) RestoreGraphicsState();
    }
}
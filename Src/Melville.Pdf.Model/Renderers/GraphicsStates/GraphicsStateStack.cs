using System;
using System.Collections.Generic;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

public sealed partial class GraphicsStateStack : IGraphiscState, IDisposable
{
    private readonly Stack<GraphicsState> states;
    public GraphicsState Current() => states.Peek();
   
    public GraphicsStateStack()
    {
        states = new ();
        states.Push(new GraphicsState());
    }

    public void SaveGraphicsState()
    {
        var newTop = new GraphicsState();
        newTop.CopyFrom(Current());
        newTop.MakeFontNotDisposable();
        states.Push(newTop);
    }

    public void RestoreGraphicsState()
    {
        (Current().Typeface as IDisposable)?.Dispose();
        states.Pop();
    }

    [DelegateTo]
    private IGraphiscState topState => states.Peek();

    public void Dispose()
    {
        while (states.Count > 0) RestoreGraphicsState();
    }
}
using System.Collections.Generic;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

public partial class GraphicsStateStack<TTypeface> : IGraphiscState<TTypeface>
{
    private readonly Stack<GraphicsState<TTypeface>> states;
    public GraphicsState<TTypeface> Current() => states.Peek();
   
    public GraphicsStateStack()
    {
        states = new ();
        states.Push(new GraphicsState<TTypeface>());
    }

    public void SaveGraphicsState()
    {
        var newTop = new GraphicsState<TTypeface>();
        newTop.CopyFrom(Current());
        states.Push(newTop);
    }

    public void RestoreGraphicsState() => states.Pop();

    [DelegateTo]
    private IGraphiscState<TTypeface> topState => states.Peek();
}
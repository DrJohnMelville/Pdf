using System.Collections.Generic;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

public partial class GraphicsStateStack : IGraphiscState
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
        states.Push(newTop);
    }

    public void RestoreGraphicsState() => states.Pop();

    [DelegateTo]
    private IGraphiscState topState => states.Peek();
}
using System;
using System.Collections.Generic;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

public partial class StackTransitionEventArgs<T>: EventArgs
{
    [FromConstructor] public T Context { get; } 
}

public sealed partial class GraphicsStateStack<T> : IGraphicsState, IDisposable
    where T: GraphicsState, new()
{ 
    private readonly Stack<T> states;

    [DelegateTo]
    private IGraphicsState TopState() => StronglyTypedCurrentState();
    public T StronglyTypedCurrentState() => states.Peek();

    public event EventHandler<StackTransitionEventArgs<T>>? ContextPushed;
    public event EventHandler<StackTransitionEventArgs<T>>? BeforeContextPopped; 

    public GraphicsStateStack()
    {
        states = new ();
        states.Push(new T());
    }

    public void SaveGraphicsState()
    {
        var newTop = new T();
        newTop.CopyFrom(StronglyTypedCurrentState());
        states.Push(newTop);
        ContextPushed?.Invoke(this, WrapArgs(newTop));
    }

    public void RestoreGraphicsState()
    {
        BeforeContextPopped?.Invoke(this, WrapArgs(StronglyTypedCurrentState()));
        StronglyTypedCurrentState().Dispose();
        states.Pop();
    }

    public void Dispose()
    {
        while (states.Count > 0) RestoreGraphicsState();
    }
    
    private StackTransitionEventArgs<T> WrapArgs(T args) => new StackTransitionEventArgs<T>(args);
}
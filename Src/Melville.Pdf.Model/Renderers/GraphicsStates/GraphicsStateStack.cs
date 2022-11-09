using System;
using System.Collections.Generic;
using System.Numerics;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

public partial class StackTransitionEventArgs<T>: EventArgs
{
    [FromConstructor] public T Context { get; } 
}

public partial class TransformPushedEventArgs : EventArgs
{
    [FromConstructor] public Matrix3x2 NewMatrix { get; }
    [FromConstructor] public Matrix3x2 CumulativeMatrix { get; }
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
    public event EventHandler<TransformPushedEventArgs>? TransformPushed; 

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
        ContextPushed?.Invoke(this, new StackTransitionEventArgs<T>(newTop));
    }

    public void RestoreGraphicsState()
    {
        BeforeContextPopped?.Invoke(this, new StackTransitionEventArgs<T>(StronglyTypedCurrentState()));
        StronglyTypedCurrentState().Dispose();
        states.Pop();
    }

    public void ModifyTransformMatrix(in Matrix3x2 newTransform)
    {
        TopState().ModifyTransformMatrix(newTransform);
        TransformPushed?.Invoke(this, new TransformPushedEventArgs(newTransform, 
            StronglyTypedCurrentState().TransformMatrix));
    }

    public void Dispose()
    {
        while (states.Count > 0) RestoreGraphicsState();
    }
}
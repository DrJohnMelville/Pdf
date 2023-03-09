using System;
using System.Collections.Generic;
using System.Numerics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

/// <summary>
/// This represents a stack of graphics states.  Pushed states copy the top state and
/// then can be popped off.
/// </summary>
/// <typeparam name="T">A GraphicsState descendant representing graphics state
/// for a specific renderer.</typeparam>
public sealed partial class GraphicsStateStack<T> : IGraphicsState, IStateChangingOperations, IDisposable
    where T: GraphicsState, new()
{ 
    private readonly Stack<T> states;

    [DelegateTo]
    private IGraphicsState TopState() => StronglyTypedCurrentState();
    /// <summary>
    /// The topmost current graphics state.
    /// </summary>
    public T StronglyTypedCurrentState() => states.Peek();

    /// <summary>
    /// Occurs just after a new context gets pushed.
    /// </summary>
    public event EventHandler<StackTransitionEventArgs<T>>? ContextPushed;

    /// <summary>
    /// Occurs just before an old context gets popped off the stack.
    /// </summary>
    public event EventHandler<StackTransitionEventArgs<T>>? BeforeContextPopped;

    /// <summary>
    /// Occurs when a new matrix is popped.
    /// </summary>
    public event EventHandler<TransformPushedEventArgs>? TransformPushed; 

    /// <summary>
    /// Construct a new GraphicsStateStack
    /// </summary>
    public GraphicsStateStack()
    {
        states = new ();
        states.Push(new T());
    }

    /// <summary>
    /// Push a fresh graphics state on top of the stack.
    /// </summary>
    public void SaveGraphicsState()
    {
        var newTop = new T();
        newTop.CopyFrom(StronglyTypedCurrentState());
        states.Push(newTop);
        ContextPushed?.Invoke(this, new StackTransitionEventArgs<T>(newTop));
    }

    /// <summary>
    /// Top the top graphics state from the top of the stack.
    /// </summary>
    public void RestoreGraphicsState()
    {
        BeforeContextPopped?.Invoke(this, new StackTransitionEventArgs<T>(StronglyTypedCurrentState()));
        StronglyTypedCurrentState().Dispose();
        states.Pop();
    }


    /// <inheritdoc />
    public void ModifyTransformMatrix(in Matrix3x2 newTransform)
    {
        TopState().ModifyTransformMatrix(newTransform);
        TransformPushed?.Invoke(this, new TransformPushedEventArgs(newTransform, 
            StronglyTypedCurrentState().TransformMatrix));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        while (states.Count > 0) RestoreGraphicsState();
    }
}
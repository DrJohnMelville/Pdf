using System;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

/// <summary>
/// This is a class that holds message parameters when a context state is pushed or popped
/// </summary>
/// <typeparam name="T"></typeparam>
public partial class StackTransitionEventArgs<T>: EventArgs
{
    /// <summary>
    /// The context that was pushed or popped.
    /// </summary>
    [FromConstructor] public T Context { get; } 
}
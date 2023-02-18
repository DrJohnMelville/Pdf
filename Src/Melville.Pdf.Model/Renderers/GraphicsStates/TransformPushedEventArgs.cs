using System;
using System.Numerics;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

/// <summary>
/// This class holds events when the transform is pushed.
/// </summary>
public partial class TransformPushedEventArgs : EventArgs
{
    /// <summary>
    /// The new matrix that was pushed.
    /// </summary>
    [FromConstructor] public Matrix3x2 NewMatrix { get; }
    /// <summary>
    /// The new cumulative view matrix.
    /// </summary>
    [FromConstructor] public Matrix3x2 CumulativeMatrix { get; }
}
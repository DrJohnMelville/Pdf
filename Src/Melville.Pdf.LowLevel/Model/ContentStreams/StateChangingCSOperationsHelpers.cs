using System;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

/// <summary>
/// Xonvenience methods for generating files with certian drawing properites.
/// </summary>
public static class StateChangingCSOperationsHelpers
{

    //This extension method is essentially a no-op it exists only to add a hint to the
    //intellisense that we might want to use a built in font name for this method
    /// <summary>
    /// Set the current font in the receiver
    /// </summary>
    /// <param name="target">The receiver whose font is to be set.</param>
    /// <param name="fontName">Name of the desired built in font.</param>
    /// <param name="size">Desired font size</param>
    public static void SetFont(
        this IStateChangingOperations target, BuiltInFontName fontName, double size) =>
        target.SetFont(fontName, size);
    
    /// <summary>
    /// Sets the line dash pattern in the receiver.
    /// </summary>
    /// <param name="target">Receiver whose lint dash pattern will be set.</param>
    /// <param name="dashPhase">The initial phase of the dashes</param>
    /// <param name="dashArray">The dash pattern.</param>
    public static void SetLineDashPattern(
        this IStateChangingOperations target, double dashPhase = 0, params double[] dashArray) =>
        target.SetLineDashPattern(dashPhase, dashArray.AsSpan());
    
}
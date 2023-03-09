namespace Melville.Pdf.Model.Renderers.GraphicsStates;

/// <summary>
/// Determines whether writing flows from left to right or top to bottom.
/// As of 2/18/2023 only left to write writing mode is actually implemented
/// </summary>
public enum WritingMode
{
    /// <summary>
    /// Wrting proceeds left to right.
    /// </summary>
    LeftToRight = 0,
    /// <summary>
    /// Not supported as of 3/1/2023 but writing proceeds top to bottom.
    /// </summary>
    TopToBottom = 1
}
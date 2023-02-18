namespace Melville.Pdf.Model.Renderers.GraphicsStates;

/// <summary>
/// Determines whether writing flows from left to right or top to bottom.
/// As of 2/18/2023 only left to write writing mode is actually implemented
/// </summary>
public enum WritingMode
{
    LeftToRight = 0,
    TopToBottom = 1
}
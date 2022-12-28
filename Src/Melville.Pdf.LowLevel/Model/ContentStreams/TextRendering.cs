namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public enum TextRendering
{
    /// <summary>
    /// Paint the internals of characters.
    /// </summary>
    Fill = 0,
    /// <summary>
    /// Stroke the outline of characters,
    /// </summary>
    Stroke = 1,
    /// <summary>
    /// Paint the interior and then stroke the exterior of characters
    /// </summary>
    FillAndStroke = 2,
    /// <summary>
    /// Update text metrics for the characters without painting anything.
    /// </summary>
    Invisible = 3,
    /// <summary>
    /// Fill characters and then set current clipping region to the text region.
    /// </summary>
    FillAndClip = 4,
    /// <summary>
    /// Stroke the outline of each character, and then set the clipping region to the text
    /// </summary>
    StrokeAndClip = 5,
    /// <summary>
    /// Fill each character, stroke the outline, and then set the clipping region to the text
    /// </summary>
    FillStrokeAndClip = 6,
    /// <summary>
    /// Set the current clipping region to the text.
    /// </summary>
    Clip = 7
}
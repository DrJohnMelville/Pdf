namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public enum LineJoinStyle
{
    /// <summary>
    /// Line ends join with a filled triangle between the extensions of the external borders of the two lines.
    /// </summary>
    Miter = 0,
    /// <summary>
    /// Lines join with a circular dot who's radius is half the line width.
    /// </summary>
    Round = 1,
    /// <summary>
    /// Lines join with a  truncated triangular bevel
    /// </summary>
    Bevel= 2
}
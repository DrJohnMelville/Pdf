namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public enum LineCap
{
    /// <summary>
    /// No end caps are drawn
    /// </summary>
    Butt = 0,
    /// <summary>
    /// A semicircle with radius line width/2 is drawn at the end of each line
    /// </summary>
    Round = 1,
    /// <summary>
    /// Rectangles one half line width in width are added to the end of each line.
    /// </summary>
    Square = 2
}
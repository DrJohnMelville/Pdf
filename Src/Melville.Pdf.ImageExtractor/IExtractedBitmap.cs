using System.Numerics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Renderers.Bitmaps;

namespace Melville.Pdf.ImageExtractor;

/// <summary>
/// This structure represents an image which was extracted from a PDF file.  It has information
/// about the page number, location, and the image data for the image.
/// </summary>
public interface IExtractedBitmap: IPdfBitmap
{
    /// <summary>
    /// This is the 1 based page number of the page upon which the image appears.
    /// </summary>
    public int Page { get; }
    /// <summary>
    /// Position of the bottom left pixel, as rendered in page coordinates.
    /// </summary>
    public Vector2 PositionBottomLeft { get; }
    /// <summary>
    /// Position of the bottom right pixel, as rendered in page coordinates.
    /// </summary>
    public Vector2 PositionBottomRight { get; }
    /// <summary>
    /// Position of the top left pixel, as rendered in page coordinates.
    /// </summary>
    public Vector2 PositionTopLeft { get; }
    /// <summary>
    /// Position of the top right pixel, as rendered in page coordinates.
    /// </summary>
    public Vector2 PositionTopRight { get; }
}
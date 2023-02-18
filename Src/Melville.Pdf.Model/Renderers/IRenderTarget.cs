using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;

/// <summary>
/// The target of a PDF rendering operation that must be implemented too paint PDFs in different technologies.
/// </summary>
public interface IRenderTarget: IDisposable
{
    /// <summary>
    /// The current graphics state.
    /// </summary>
    IGraphicsState GraphicsState { get; }

    /// <summary>
    /// Draw a bitmap.
    /// </summary>
    /// <param name="bitmap">The IPdfBitmap to paint</param>
    ValueTask RenderBitmap(IPdfBitmap bitmap);

    /// <summary>
    /// Create a draw target that can draw shapes.
    /// </summary>
    IDrawTarget CreateDrawTarget();

    /// <summary>
    /// Setup the page and background size for the render target.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="transform"></param>
    void SetBackgroundRect(in PdfRect rect, double width, double height, in Matrix3x2 transform);
    void MapUserSpaceToBitmapSpace(in PdfRect rect, double xPixels, double yPixels, in Matrix3x2 adjustOutput);
    void CloneStateFrom(GraphicsState priorState);
    IRealizedFont WrapRealizedFont(IRealizedFont font);
}
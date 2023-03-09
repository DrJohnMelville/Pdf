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
    
    /// <summary>
    /// Setup the initial transform from a page clipping rectangle to device space.
    /// </summary>
    /// <param name="rect">The clipping rectangle in PDF coordinates</param>
    /// <param name="xPixels">The desired output width, in device coordinates</param>
    /// <param name="yPixels">The desired output height, in device coordinates</param>
    /// <param name="adjustOutput">A transform matrix that will adjust the output.  In practice
    /// this is used to implement default page rotation</param>
    void MapUserSpaceToBitmapSpace(in PdfRect rect, double xPixels, double yPixels, in Matrix3x2 adjustOutput);

    /// <summary>
    /// Close this target's graphics state from a prior state.  This is used for XForms, type 3 fonts and
    /// other items that inherit a graphics state from their context.
    /// </summary>
    /// <param name="priorState">The previous state to duiplicate.</param>
    void CloneStateFrom(GraphicsState priorState);
    /// <summary>
    /// Allows the renderer to adjust or replace every IRealizedFont used in the document.  The WPF Renderer
    /// uses this methods to wrap fonts in a renderer-specific glyph caching mechanism.
    /// </summary>
    /// <param name="font">An IRealizedFont, parsed out of a font file or stream</param>
    /// <returns>The IRealizedFont that should be used to draw characters in this font.</returns>
    IRealizedFont WrapRealizedFont(IRealizedFont font);
}
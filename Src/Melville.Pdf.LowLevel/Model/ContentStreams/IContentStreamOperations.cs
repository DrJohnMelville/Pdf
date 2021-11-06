using System.Runtime.InteropServices;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public interface IFontMetricsOperations
{
    /// <summary>
    /// Content stream operator wx wx d0
    /// </summary>
    void SetColoredGlyphMetrics(double wX, double wY);
    
    /// <summary>
    /// Content stream operator wx wy llx lly urx ury
    /// </summary>
    void SetUncoloredGlyphMetrics(double wX, double wY, double llX, double llY, double urX, double urY);
}
public interface IContentStreamOperations: 
    IStateChangingOperations, IDrawingOperations, IColorOperations, 
    ITextObjectOperations, ITextBlockOperations, IMarkedContentCSOperations, ICompatibilityOperations,
    IFontMetricsOperations
{
}
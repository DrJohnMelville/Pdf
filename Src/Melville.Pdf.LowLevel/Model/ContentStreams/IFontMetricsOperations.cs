namespace Melville.Pdf.LowLevel.Model.ContentStreams;

/// <summary>
/// Operators used by type 3 fonts to declare their font metrics.
/// </summary>
public interface IFontMetricsOperations
{
    /// <summary>
    /// Content stream operator wx wx d0
    /// </summary>
    void SetColoredGlyphMetrics(double wX, double wY);
    
    /// <summary>
    /// Content stream operator wx wy llx lly urx ury d1
    /// </summary>
    void SetUncoloredGlyphMetrics(double wX, double wY, double llX, double llY, double urX, double urY);
}
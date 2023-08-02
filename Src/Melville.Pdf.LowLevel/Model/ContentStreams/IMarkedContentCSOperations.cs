using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

/// <summary>
/// Content Stream Operators for marked content
/// </summary>
public interface IMarkedContentCSOperations
{
    /// <summary>
    /// Content stream operator tag MP
    /// </summary>
    void MarkedContentPoint(PdfDirectObject tag);

    /// <summary>
    /// Content stream operator tag properties MP
    /// </summary>
    ValueTask MarkedContentPointAsync(PdfDirectObject tag, PdfDirectObject properties);

    /// <summary>
    /// Content stream operator tag dictionaru MP
    /// </summary>
    ValueTask MarkedContentPointAsync(PdfDirectObject tag, PdfDictionary dictionary);

    /// <summary>
    /// Content stream operator tag BMC
    /// </summary>
    void BeginMarkedRange(PdfDirectObject tag);

    /// <summary>
    /// Content stream operator tag dictName BDC
    /// </summary>
    ValueTask BeginMarkedRangeAsync(PdfDirectObject tag, PdfDirectObject dictName);

    /// <summary>
    /// Content stream operator tag  inlineDicitionary BDC
    /// </summary>
    ValueTask BeginMarkedRangeAsync(PdfDirectObject tag, PdfDictionary dictionary);

    /// <summary>
    /// Content stream operator EMC
    /// </summary>
    void EndMarkedRange();
}
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

/// <summary>
/// Content Stream Operators for marked content
/// </summary>
public interface IMarkedContentCSOperations
{
    /// <summary>
    /// Content stream operator tag MP
    /// </summary>
    void MarkedContentPoint(PdfDirectValue tag);

    /// <summary>
    /// Content stream operator tag properties MP
    /// </summary>
    ValueTask MarkedContentPointAsync(PdfDirectValue tag, PdfDirectValue properties);

    /// <summary>
    /// Content stream operator tag dictionaru MP
    /// </summary>
    ValueTask MarkedContentPointAsync(PdfDirectValue tag, PdfValueDictionary dictionary);

    /// <summary>
    /// Content stream operator tag BMC
    /// </summary>
    void BeginMarkedRange(PdfDirectValue tag);

    /// <summary>
    /// Content stream operator tag dictName BDC
    /// </summary>
    ValueTask BeginMarkedRangeAsync(PdfDirectValue tag, PdfDirectValue dictName);

    /// <summary>
    /// Content stream operator tag  inlineDicitionary BDC
    /// </summary>
    ValueTask BeginMarkedRangeAsync(PdfDirectValue tag, PdfValueDictionary dictionary);

    /// <summary>
    /// Content stream operator EMC
    /// </summary>
    void EndMarkedRange();
}
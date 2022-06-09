using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public interface IMarkedContentCSOperations
{
    /// <summary>
    /// Content stream operator tag MP
    /// </summary>
    void MarkedContentPoint(PdfName tag);

    /// <summary>
    /// Content stream operator tag properties MP
    /// </summary>
    ValueTask MarkedContentPointAsync(PdfName tag, PdfName properties);

    /// <summary>
    /// Content stream operator tag dictionaru MP
    /// </summary>
    ValueTask MarkedContentPointAsync(PdfName tag, PdfDictionary dictionary);

    /// <summary>
    /// Content stream operator tag BMC
    /// </summary>
    void BeginMarkedRange(PdfName tag);

    /// <summary>
    /// Content stream operator tag dictName BDC
    /// </summary>
    ValueTask BeginMarkedRangeAsync(PdfName tag, PdfName dictName);

    /// <summary>
    /// Content stream operator tag  inlineDicitionary BDC
    /// </summary>
    ValueTask BeginMarkedRangeAsync(PdfName tag, PdfDictionary dictionary);

    /// <summary>
    /// Content stream operator EMC
    /// </summary>
    void EndMarkedRange();
}
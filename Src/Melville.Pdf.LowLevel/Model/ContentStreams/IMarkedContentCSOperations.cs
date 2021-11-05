using Melville.Pdf.LowLevel.Model.Conventions;
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
    void MarkedContentPoint(PdfName tag, PdfName properties);
}

public static class MarkedContentCSOperationsHelpers
{
}
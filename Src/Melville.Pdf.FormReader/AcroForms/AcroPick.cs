using Melville.INPC;

namespace Melville.Pdf.FormReader.AcroForms
{
    internal abstract partial class AcroPick: AcroFormField, IPdfPick
    {
        [FromConstructor] public IReadOnlyList<PdfPickOption> Options { get; }
    }
}
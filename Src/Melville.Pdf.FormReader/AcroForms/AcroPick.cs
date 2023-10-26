using Melville.INPC;
using Melville.Pdf.FormReader.Interface;

namespace Melville.Pdf.FormReader.AcroForms
{
    internal abstract partial class AcroPick: AcroFieldWithAppearance, IPdfPick
    {
        [FromConstructor] public IReadOnlyList<PdfPickOption> Options { get; }
    }
}
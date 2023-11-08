using Melville.INPC;
using Melville.Pdf.FormReader.Interface;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.FormReader.XfaForms
{
    internal partial class NonMirroredXfaValue : IPdfFormField
    {
        [FromConstructor] public string Name { get; }
        public PdfDirectObject Value { get; set; }
    }
}
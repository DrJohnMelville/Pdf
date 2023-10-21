using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.FormReader.AcroForms
{
    [FromConstructor]
    internal partial class AcroTextBox : AcroFormField, IPdfTextBox
    {
        public string StringValue
        {
            get => Value.ToString();
            set => Value =  PdfDirectObject.CreateString(value.AsExtendedAsciiBytes());
        }
    }
}
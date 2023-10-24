using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.FormReader.AcroForms;

[FromConstructor]
internal partial class AcroTextBox : AcroFormField, IPdfTextBox
{
    public string StringValue
    {
        get => Value.DecodedString();
        set => Value =  PdfDirectObject.CreateUtf8String(value);
    }

    protected override ValueTask UpdateAppearance(ICanReplaceObjects target) => 
        ReplaceTextAppearance(target);
}
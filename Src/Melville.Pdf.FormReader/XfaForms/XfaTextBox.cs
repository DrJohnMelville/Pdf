using Melville.Pdf.FormReader.Interface;
using System.Xml.Linq;
using Melville.INPC;
using Melville.Pdf.FormReader.AcroForms;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

namespace Melville.Pdf.FormReader.XfaForms;



[FromConstructor]
internal partial class XfaTextBox: XfaControl,  IPdfTextBox
{

    public string StringValue
    {
        get => backingField.Value.DecodedString(); 
        set => backingField.Value = PdfDirectObject.CreateUtf8String(value);
    }
}

[FromConstructor]
internal partial class XfaSinglePick: XfaControl, IPdfSinglePick
{
    [FromConstructor] public IReadOnlyList<PdfPickOption> Options { get; }

    public PdfPickOption? Selected
    {
        get => SinglePickImplementation.FindCurrentItem(backingField.Value, Options);
        set => SinglePickImplementation.SetCurrentItem(backingField, value);
    }
}
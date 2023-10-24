using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.FormReader.AcroForms;

[FromConstructor]
internal partial class AcroSingleChoice : AcroPick, IPdfSinglePick
{
    public PdfPickOption? Selected
    {
        get => Options.FirstOrDefault(i=>i.Value.Equals(Value)); 
        set => Value = value?.Value ?? PdfDirectObject.CreateNull();
    }

    protected override ValueTask UpdateAppearance(
        ICanReplaceObjects target, PdfDirectObject formAppearanceString) => 
        ReplaceTextAppearance(target, formAppearanceString);

}
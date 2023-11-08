using Melville.INPC;
using Melville.Pdf.FormReader.Interface;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.FormReader.AcroForms;

[FromConstructor]
internal partial class AcroSingleChoice : AcroPick, IPdfSinglePick
{
    public PdfPickOption? Selected
    {
        get => SinglePickImplementation.FindCurrentItem(Value, Options); 
        set => SinglePickImplementation.SetCurrentItem(this, value);
    }


    protected override ValueTask UpdateAppearanceAsync(
        ICanReplaceObjects target, PdfDirectObject formAppearanceString) => 
        ReplaceTextAppearanceAsync(target, formAppearanceString);

}

internal static class SinglePickImplementation
{
    public static PdfPickOption? FindCurrentItem(
        PdfDirectObject value, IReadOnlyList<PdfPickOption> options)
    {
        using var valueString = value.DecodedBuffer();
        return options.FirstOrDefault(i=>
        {
            using var item = i.Value.DecodedBuffer();
            return item.Span.SequenceEqual(valueString.Span);
        });
    }

    public static void SetCurrentItem(IPdfFormField target, PdfPickOption? choice) =>
        target.Value = choice?.Value ?? PdfDirectObject.CreateNull();

}
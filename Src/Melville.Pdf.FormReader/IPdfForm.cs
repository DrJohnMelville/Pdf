using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.FormReader;

public interface IPdfForm
{
    IReadOnlyList<IPdfFormField> Fields { get; }
    PdfLowLevelDocument CreateModifiedDocument();
}

public interface IPdfFormField
{
    string Name { get; }
    PdfDirectObject Value { get; set; }
}

public interface IPdfTextBox : IPdfFormField
{
    string StringValue { get; set; }
}

public interface IPdfCheckBox : IPdfFormField
{
    bool IsChecked { get; set; }
}

public partial class PdfPickOption
{
    [FromConstructor] public string Title { get; }
    [FromConstructor] public PdfDirectObject Value { get; }
}

public interface IPdfPick : IPdfFormField
{
    IReadOnlyList<PdfPickOption> Options { get; }
}

public interface IPdfSinglePick: IPdfPick
{
    public PdfPickOption? Selected { get; set; }
}

public interface IPdfMultiPick : IPdfPick
{
    public IList<PdfPickOption> Selected { get; }
}
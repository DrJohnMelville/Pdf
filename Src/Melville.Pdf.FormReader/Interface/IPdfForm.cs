using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Document;

namespace Melville.Pdf.FormReader.Interface;

/// <summary>
/// This interface represents a fillable pdf form.
/// </summary>
public interface IPdfForm
{
    /// <summary>
    /// A list of fields in the form.
    /// </summary>
    IReadOnlyList<IPdfFormField> Fields { get; }
    /// <summary>
    /// Create a low level PdfDocument that represents the filled out form.
    /// </summary>
    /// <returns>A new Pdf LowLevelDocument with form fields filled out.  If there are no
    /// fields, this method may return the source lowlevel document.</returns>
    ValueTask<PdfLowLevelDocument> CreateModifiedDocumentAsync();
}

internal partial class EmptyPdfForm: IPdfForm
{
    [FromConstructor] private readonly PdfLowLevelDocument document;
    /// <inheritdoc />
    public IReadOnlyList<IPdfFormField> Fields => [];

    /// <inheritdoc />
    public ValueTask<PdfLowLevelDocument> CreateModifiedDocumentAsync() =>
        ValueTask.FromResult(document);
}
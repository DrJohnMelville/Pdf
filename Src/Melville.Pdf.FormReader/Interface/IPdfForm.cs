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
    /// <returns>A new Pdf LowLevelDocument with form fields filled out.</returns>
    ValueTask<PdfLowLevelDocument> CreateModifiedDocumentAsync();
}
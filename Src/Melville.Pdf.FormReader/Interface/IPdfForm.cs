using Melville.Pdf.FormReader.AcroForms;
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

public static class PdfFormOperations
{
    public static IPdfFormField NameToField(this IPdfForm form, string name) =>
        form.Fields[form.NameToFieldIndex(name)];

    public static int NameToFieldIndex(this IPdfForm form, string name) =>
        form.Fields.NameToFieldIndex(name);
    
    public static int NameToFieldIndex(this IReadOnlyList<IPdfFormField> form, string name)
    {
        for (int i = 0; i < form.Count; i++)
        {
            if (name.AsSpan().SameXfaNameAs(form[i].Name)) return i;
        }

        return -1;

    }
}
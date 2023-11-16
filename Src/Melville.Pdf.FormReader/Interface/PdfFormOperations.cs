using Melville.Pdf.FormReader.AcroForms;

namespace Melville.Pdf.FormReader.Interface;

/// <summary>
/// Methods to get fields from a IPdfForm while observing the PDF form naming conventions
/// </summary>
public static class PdfFormOperations
{
    /// <summary>
    /// Get a form field from a IPdf Form
    /// </summary>
    /// <param name="form">The form to search.</param>
    /// <param name="name">The name of the requested field,.</param>
    /// <returns>The first field matching the given name</returns>
    /// <exception cref="IndexOutOfRangeException">If the requested field does not exist</exception>
    public static IPdfFormField NameToField(this IPdfForm form, string name) =>
        form.Fields[form.NameToFieldIndex(name)];

    /// <summary>
    /// Gets the index of the named field in a PDF form
    /// </summary>
    /// <param name="form">The form to search</param>
    /// <param name="name">The name of the requested field.</param>
    /// <returns>The index of the first field matching the name given, or
    /// -1 if no field matches.</returns>
    public static int NameToFieldIndex(this IPdfForm form, string name) =>
        form.Fields.NameToFieldIndex(name);

    /// <summary>
    /// Gets the index of the named field in a read only list of PDF fields.
    /// </summary>
    /// <param name="form">The form to search</param>
    /// <param name="name">The name of the requested field.</param>
    /// <returns>The index of the first field matching the name given, or
    /// -1 if no field matches.</returns>
    public static int NameToFieldIndex(this IReadOnlyList<IPdfFormField> form, string name)
    {
        for (int i = 0; i < form.Count; i++)
        {
            if (name.AsSpan().SameXfaNameAs(form[i].Name)) return i;
        }

        return -1;

    }
}
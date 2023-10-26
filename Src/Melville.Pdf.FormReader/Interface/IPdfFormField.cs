using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.FormReader.Interface
{
    /// <summary>
    /// Represents a field in a PDF form.
    /// </summary>
    public interface IPdfFormField
    {
        /// <summary>
        /// PDF name of the field
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Get or set the value of the field using PDF conventions
        /// </summary>
        PdfDirectObject Value { get; set; }
    }
}
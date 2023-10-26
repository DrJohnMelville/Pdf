namespace Melville.Pdf.FormReader.Interface
{
    /// <summary>
    /// Represents a checkbox in a PDF form.
    /// </summary>
    public interface IPdfCheckBox : IPdfFormField
    {
        /// <summary>
        /// Get or set the Checked state of the checkbox.
        /// </summary>
        bool IsChecked { get; set; }
    }
}
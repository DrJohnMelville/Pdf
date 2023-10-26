namespace Melville.Pdf.FormReader.Interface
{
    /// <summary>
    /// Represents a text box in a PDF form.
    /// </summary>
    public interface IPdfTextBox : IPdfFormField
    {
        /// <summary>
        /// Get or set the text in the box
        /// </summary>
        string StringValue { get; set; }
    }
}
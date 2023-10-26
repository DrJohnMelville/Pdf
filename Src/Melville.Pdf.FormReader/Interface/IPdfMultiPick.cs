namespace Melville.Pdf.FormReader.Interface
{
    /// <summary>
    /// Represents a multi-selection pick form control
    /// </summary>
    public interface IPdfMultiPick : IPdfPick
    {
        /// <summary>
        /// The list of selected items.
        /// </summary>
        public IList<PdfPickOption> Selected { get; }
    }
}
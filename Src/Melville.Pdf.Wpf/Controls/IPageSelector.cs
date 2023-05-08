namespace Melville.Pdf.Wpf.Controls
{
    /// <summary>
    /// This interface represents UI that can select the displayed page within a PDF document.
    /// </summary>
    public interface IPageSelector
    {
        /// <summary>
        /// The page currently being dispplayed.  The first page is number 1.
        /// </summary>
        int Page { get; set; }
        /// <summary>
        /// Total number of pages in the document, which is also the number of the last page.
        /// </summary>
        int MaxPage { get; set; }
        /// <summary>
        /// Go to the next page.
        /// </summary>
        void Increment();
        /// <summary>
        /// Go to the last page;
        /// </summary>
        void ToEnd();
        /// <summary>
        /// Go to the previous page.
        /// </summary>
        void Decrement();
        /// <summary>
        /// Go to the first page.
        /// </summary>
        void ToStart();
        /// <summary>
        /// Set the displayed page without sending a property changed notification.
        /// </summary>
        /// <param name="newPageNumber">Page to display.  First page is page 1.</param>
        public void SetPageSilent(int newPageNumber);
    }
}
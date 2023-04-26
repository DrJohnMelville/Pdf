using System.Windows.Controls;
using System.Windows.Input;
using Accessibility;
using Melville.Hacks;
using Melville.INPC;

namespace Melville.Pdf.Wpf.Controls;

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

public partial class PageSelectorViewModel: IPageSelector
{
    /// <summary>
    /// The current selected page number
    /// </summary>
    [AutoNotify] private int page = 1;
    /// <summary>
    /// The number of the first page.
    /// </summary>
    [AutoNotify] private int minPage = 1;
    /// <summary>
    /// The number of the last page.
    /// </summary>
    [AutoNotify] private int maxPage = 1;
    private int PageSetFilter(int newPage) => newPage.Clamp(MinPage, MaxPage);
    private void OnMaxPageChanged(int newValue) => page = minPage;

    public ICommand IncrementCommand { get; } 
    public ICommand DecrementCommand { get; } 
    public ICommand ToStartCommand { get; } 
    public ICommand ToEndCommand { get; } 

    public PageSelectorViewModel()
    {
        IncrementCommand = new SimpleCommand(_ => Increment(), _ => Page < MaxPage, this);
        DecrementCommand = new SimpleCommand(_ => Decrement(), _ => Page > MinPage, this );
        ToStartCommand = new SimpleCommand(_ => ToStart(), _ => Page != MinPage, this);
        ToEndCommand = new SimpleCommand(_=>ToEnd(), _ => Page < MaxPage, this);
    }

    /// <inheritdoc />
    public void Increment() => Page++;
    /// <inheritdoc />
    public void Decrement() => Page--;
    /// <inheritdoc />
    public void ToStart() => Page = MinPage;
    /// <inheritdoc />
    public void ToEnd() => Page = MaxPage;

    /// <summary>
    /// String to display in the textbox containing the current and total number of pages.
    /// </summary>
    public string DisplayString => $"{Page} of {MaxPage}";
    /// <inheritdoc />
    public void SetPageSilent(int newPageNumber) => page = newPageNumber;
}
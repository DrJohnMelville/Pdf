using System.Windows.Controls;
using System.Windows.Input;
using Accessibility;
using Melville.Hacks;
using Melville.INPC;

namespace Melville.Pdf.Wpf.Controls;

/// <summary>
/// This view model allows the user to see and change the current page.
/// </summary>
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

    /// <summary>
    /// Go to the next page
    /// </summary>
    public ICommand IncrementCommand { get; } 
    /// <summary>
    /// Go to the prior page.
    /// </summary>
    public ICommand DecrementCommand { get; } 
    /// <summary>
    /// Go to the first page of the document
    /// </summary>
    public ICommand ToStartCommand { get; } 
    /// <summary>
    /// Go to the last page.
    /// </summary>
    public ICommand ToEndCommand { get; } 

    /// <summary>
    /// Default Constructor
    /// </summary>
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
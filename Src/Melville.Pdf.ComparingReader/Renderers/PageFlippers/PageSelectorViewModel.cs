using System;
using Melville.Hacks;
using Melville.INPC;

namespace Melville.Pdf.ComparingReader.Renderers.PageFlippers;

public interface IPageSelector
{
    int Page { get; set; }
    int MaxPage { get; set; }
    void Increment();
    void ToEnd();
    void Decrement();
    void ToStart();
    public void ToStartSilent();
}
public partial class PageSelectorViewModel: IPageSelector
{
    [AutoNotify] private int page = 1;
    [AutoNotify] private int maxPage = 1;
    private int PageSetFilter(int newPage) => newPage.Clamp(1, MaxPage);
    private void OnMaxPageChanged(int newValue) => page = 1;

    public void Increment() => Page++;
    public void ToEnd() => Page = MaxPage;

    public void Decrement() => Page--;

    public void ToStart() => Page = 1;

    [AutoNotify] public string DisplayString => $"{Page} of {MaxPage}";
    [AutoNotify] public bool CanPrev => Page > 1;
    [AutoNotify] public bool CanNext => Page < MaxPage;
    public void ToStartSilent() => page = 1;
}
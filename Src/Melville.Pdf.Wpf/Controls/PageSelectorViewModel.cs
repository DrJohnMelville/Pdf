using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using Accessibility;
using Melville.Hacks;
using Melville.INPC;

namespace Melville.Pdf.Wpf.Controls;

public interface IPageSelector
{
    int Page { get; set; }
    int MaxPage { get; set; }
    void Increment();
    void ToEnd();
    void Decrement();
    void ToStart();
    public void SetPageSilent(int newPageNumber);
}

public class SimpleCommand : ICommand
{
    private readonly Action<object?> execute;
    private readonly Func<object?, bool> canExecute;

    public SimpleCommand(Action<object?> execute, Func<object?, bool> canExecute, INotifyPropertyChanged target)
    {
        this.execute = execute;
        this.canExecute = canExecute;
        target.PropertyChanged += (_, _) => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool CanExecute(object? parameter) => canExecute(parameter);

    public void Execute(object? parameter) => execute(parameter);

    public event EventHandler? CanExecuteChanged;
}
public partial class PageSelectorViewModel: IPageSelector
{
    [AutoNotify] private int page = 1;
    [AutoNotify] private int maxPage = 1;
    [AutoNotify] private int minPage = 1;
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

    public void Increment() => Page++;
    public void Decrement() => Page--;
    public void ToStart() => Page = MinPage;
    public void ToEnd() => Page = MaxPage;

    [AutoNotify] public string DisplayString => $"{Page} of {MaxPage}";
    public void SetPageSilent(int newPageNumber) => page = newPageNumber;
}
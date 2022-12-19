using System.ComponentModel;
using System.Windows.Input;

namespace Melville.Pdf.Wpf.Controls;

internal class SimpleCommand : ICommand
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
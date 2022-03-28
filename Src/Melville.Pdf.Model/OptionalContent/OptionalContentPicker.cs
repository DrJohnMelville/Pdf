using System.Collections.Generic;
using System.ComponentModel;
using Melville.INPC;

namespace Melville.Pdf.Model.OptionalContent;

public interface IOptionalContentDisplayGroup
{
    string Name { get; }
    bool Visible { get; set; }
    bool ShowCheck { get; }
    IReadOnlyList<IOptionalContentDisplayGroup> Children { get; }
}
public class OptionalContentPickerTitle:IOptionalContentDisplayGroup
{
    public string Name { get; }
    public bool Visible { get; set; }
    public bool ShowCheck => false;
    public IReadOnlyList<IOptionalContentDisplayGroup> Children { get; }

    public OptionalContentPickerTitle(string name, IReadOnlyList<IOptionalContentDisplayGroup> children)
    {
        Name = name;
        Children = children;
    }
}

public class OptionalContentPickerSuperClass : IOptionalContentDisplayGroup, IExternalNotifyPropertyChanged
{
    private OptionalGroup title;
    public string Name => title.Name;
    public bool Visible
    {
        get => title.Visible; 
        set => title.Visible = value;
    }
    public bool ShowCheck => true;
    public IReadOnlyList<IOptionalContentDisplayGroup> Children { get; }

    public OptionalContentPickerSuperClass(OptionalGroup title, IReadOnlyList<IOptionalContentDisplayGroup> children)
    {
        this.title = title;
        Children = children;
        this.DelegatePropertyChangeFrom(title, nameof(Visible), nameof(Visible));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void IExternalNotifyPropertyChanged.OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
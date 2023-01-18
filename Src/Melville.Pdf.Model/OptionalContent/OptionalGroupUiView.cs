using System.Collections.Generic;
using System.ComponentModel;
using Melville.INPC;

namespace Melville.Pdf.Model.OptionalContent;

internal class OptionalGroupUiView : IOptionalContentDisplayGroup, IExternalNotifyPropertyChanged
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

    public OptionalGroupUiView(OptionalGroup title, IReadOnlyList<IOptionalContentDisplayGroup> children)
    {
        this.title = title;
        Children = children;
        this.DelegatePropertyChangeFrom(title, nameof(Visible), nameof(Visible));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void IExternalNotifyPropertyChanged.OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
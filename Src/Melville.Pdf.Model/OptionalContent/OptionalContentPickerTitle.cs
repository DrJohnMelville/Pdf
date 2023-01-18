using System.Collections.Generic;

namespace Melville.Pdf.Model.OptionalContent;

internal class OptionalContentPickerTitle:IOptionalContentDisplayGroup
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
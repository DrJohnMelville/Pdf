using System;
using System.Collections.Generic;
using Melville.INPC;

namespace Melville.Pdf.Model.OptionalContent;

public partial class OptionalGroup: IOptionalContentDisplayGroup
{
    public string Name { get; }
    [AutoNotify] private bool visible = false;
    public virtual bool ShowCheck => true;

    public virtual IReadOnlyList<IOptionalContentDisplayGroup> Children =>
        Array.Empty<IOptionalContentDisplayGroup>();

    public OptionalGroup(string name)
    {
        Name = name;
    }
}
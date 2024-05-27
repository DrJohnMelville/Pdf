using Melville.Fonts;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

[AutoNotify]
public partial class MultiStringViewModel (
    Func<ValueTask<IReadOnlyList<string>>> source, string title)
{
    public string Title => title;

    private readonly LoadOnce<IReadOnlyList<string>> mappings = new(["Loading Mapping"]);
    public IReadOnlyList<string> Items => mappings.GetValue(this, source);
}
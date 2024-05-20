using Melville.Fonts.SfntParsers;
using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public partial class TableViewModel
{
    [FromConstructor] public SFnt font;
    [FromConstructor] public TableRecord Record { get; }
    private bool loadCalled;

    public string Title => Record.TableName;
    public string? ToolTip => Record.ToString();

    [AutoNotify] private object? details;
    private object DetailsGetFilter(object inner) => details ?? LoadTable();

    private object LoadTable()
    {
        if (!loadCalled)
        {
            loadCalled = true;
            LoadTableAsync(); // not awaited, I get the result through a property set notification
        }
        return LoadingTableViewModel.Instance;
    }

    private ValueTask LoadTableAsync()
    {
        return ValueTask.CompletedTask;
    }
}
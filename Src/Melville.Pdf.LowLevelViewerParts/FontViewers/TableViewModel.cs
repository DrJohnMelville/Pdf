using Melville.Fonts.SfntParsers;
using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.INPC;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public partial class TableViewModel
{
    [FromConstructor] public SFnt font;
    [FromConstructor] public TableRecord Record { get; }
    private bool loadCalled;

    public string Title => Record.TableName;
    public string? ToolTip => Record.ToString();

    [AutoNotify] private object? details;
    private object? DetailsGetFilter(object? inner) => details ?? LoadTable();

    private object LoadTable()
    {
        if (!loadCalled)
        {
            loadCalled = true;
            LoadTableAsync(); // not awaited, I get the result through a property set notification
        }
        return LoadingTableViewModel.Instance;
    }

    private async ValueTask LoadTableAsync()
    {
        var bytes = await font.GetTableBytesAsync(Record);
        Details = new ByteStringViewModel(bytes);
    }
}
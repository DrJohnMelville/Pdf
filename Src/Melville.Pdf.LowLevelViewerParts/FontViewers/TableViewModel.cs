using System.Globalization;
using Melville.Fonts.SfntParsers;
using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.INPC;
using Melville.Parsing.CountingReaders;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

[AutoNotify]
public partial class TableViewModel
{
    [FromConstructor] public SFnt font;
    [FromConstructor] public TableRecord Record { get; }
    private bool loadCalled;

    public string Title => Record.TableName;
    public string? ToolTip => Record.ToString();

    private readonly LoadOnce<object> details = new(LoadingTableViewModel.Instance);
    public object Details => details.GetValue(this, LoadTableAsync);

    private async ValueTask<object> LoadTableAsync() => 
        await SpecialTableParser.ParseAsync(Record.Tag, await font.GetTableBytesAsync(Record));
}
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

    public string Title => Record.TableName;
    public string? ToolTip => Record.ToString();

    private readonly LoadOnce<object> details = new(LoadingTableViewModel.Instance);
    public object Details => details.GetValue(this, LoadTableAsync);

    private ValueTask<object> LoadTableAsync() => SpecialTableParser.ParseAsync(Record, font);
}
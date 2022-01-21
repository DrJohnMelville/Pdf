using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.INPC;
using Melville.Linq;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.WpfViewerParts.LowLevelViewer.DocumentParts;

public class StringDocumentPart : DocumentPart
{
    public string DisplayContent { get; }
    public StringDocumentPart(PdfString title, string prefix) : base($"{prefix}({title})")
    {
        DisplayContent = String.Join(Environment.NewLine,
            title.Bytes.BinaryFormat().Select((hexDump, index) => $"{index:X7}0  {hexDump}"));
    }
}

public partial class StreamDocumentPart : DocumentPart
{
    private readonly PdfStream source;
    [AutoNotify] private string displayContent = "";
    [AutoNotify] private StreamFormat selectedFormat = StreamFormat.DiskRepresentation;

    private string DisplayContentGetFilter(string item)
    {
        if (item.Length == 0)
        {
            Task.Run(LoadBytesAsync);
        }
        return item;
    }

    private void OnSelectedFormatChanged() => DisplayContent = "";

    public StreamDocumentPart(string title, IReadOnlyList<DocumentPart> children, PdfStream source) : base(title, children)
    {
        this.source = source;
    }

    public async Task LoadBytesAsync()
    {
        Formats = (await source.GetOrNullAsync(KnownNames.Filter)).AsList()
            .Select(i => i.ToString()??"No name")
            .Prepend("Raw Stream")
            .ToArray();
        await using var streamData = await SelectedFormatContentAsync();
        var data = new byte[10240];
        var read = await streamData.FillBufferAsync(data, 0, data.Length);
        DisplayContent = string.Join("\r\n", CreateHexDump(data.Take(read)));
    }

    private ValueTask<Stream> SelectedFormatContentAsync() => source.StreamContentAsync(selectedFormat);

    public async Task SaveStreamToFile([FromServices] IOpenSaveFile fileDlg)
    {
        var target = fileDlg.GetSaveFile(null, "", "All Files (*.*)|*.*", "Write stream to file");
        if (target == null) return;
        await using var targetStream = await target.CreateWrite();
        await using var source = await SelectedFormatContentAsync();
        await source.CopyToAsync(targetStream);
    }

    private static IEnumerable<string> CreateHexDump(IEnumerable<byte> data) => 
        data.BinaryFormat().Select((hexDump, index) => $"{index:X7}0  {hexDump}");
}
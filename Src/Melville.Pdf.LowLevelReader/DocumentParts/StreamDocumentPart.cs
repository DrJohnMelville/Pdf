using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Forms;
using Melville.FileSystem;
using Melville.INPC;
using Melville.Linq;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevelReader.DocumentParts;

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
        await using var streamData = await source.StreamContentAsync(selectedFormat);
        var data = new byte[10240];
        var read = await streamData.FillBufferAsync(data, 0, data.Length);
        DisplayContent = string.Join("\r\n", CreateHexDump(data.Take(read)));
    }

    private static IEnumerable<string> CreateHexDump(IEnumerable<byte> data) => 
        data.BinaryFormat().Select((hexDump, index) => $"{index:X7}0  {hexDump}");
}
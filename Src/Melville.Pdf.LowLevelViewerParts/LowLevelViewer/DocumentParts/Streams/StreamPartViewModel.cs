﻿using System.IO;
using System.Windows;
using Melville.INPC;
using Melville.Linq;
using Melville.MVVM.Wpf.ViewFrames;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;

public record StreamDisplayFormat(string Name, Func<PdfStream, ValueTask<object>> Creator);

public partial class StreamPartViewModel : DocumentPart, ICreateView
{
    public PdfStream Source { get; }
    [AutoNotify] private IReadOnlyList<StreamDisplayFormat> formats = Array.Empty<StreamDisplayFormat>();
    [AutoNotify] private StreamDisplayFormat? selectedFormat = null;
    public override object? DetailView => this;
    [AutoNotify] private object? content;

    private async void OnSelectedFormatChanged(StreamDisplayFormat? newFormat)
    {
        if (newFormat is not null)
            Content = await newFormat.Creator(Source);
    }

    private async void LoadFormats()
    {
        var fmts = new List<StreamDisplayFormat>();
        await AddFormatsAsync(fmts);

        Formats = fmts;
        SelectedFormat = fmts.First();
    }

    protected virtual async ValueTask AddFormatsAsync(List<StreamDisplayFormat> fmts)
    {
        var fmtList = (await Source.GetOrNullAsync(KnownNames.Filter)).ObjectAsUnresolvedList();
        for (int i = fmtList.Count -1; i >= 0; i--)
        {
            var format = (StreamFormat)(i + 1);
            fmts.Add(new StreamDisplayFormat(fmtList[i].ToString() ?? "No Name",
                p => LoadBytesAsync(p, format)));
        }
        fmts.Add(new StreamDisplayFormat("Implicit Encryption",
            p => LoadBytesAsync(p, StreamFormat.ImplicitEncryption)));
        fmts.Add(new StreamDisplayFormat("Disk Representation",
            p => LoadBytesAsync(p, StreamFormat.DiskRepresentation)));
    }

    public StreamPartViewModel(string title, IReadOnlyList<DocumentPart> children, PdfStream source) : base(title, children)
    {
        this.Source = source;
        LoadFormats();
    }

    public async ValueTask<object> LoadBytesAsync(PdfStream stream, StreamFormat fmt)
    {
        await using var streamData = await stream.StreamContentAsync(fmt);
        var final = new MemoryStream();
        await streamData.CopyToAsync(final);
        return new ByteStringViewModel(final.ToArray());
    }
    
    private static IEnumerable<string> CreateHexDump(IEnumerable<byte> data) => 
        data.BinaryFormat().Select((hexDump, index) => $"{index:X7}0  {hexDump}");
    
    public UIElement View() => new StreamPartView{ DataContext = this};

}
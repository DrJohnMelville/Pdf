using System.IO;
using System.IO.Pipelines;
using System.Windows;
using Melville.FileSystem;
using Melville.INPC;
using Melville.Linq;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
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
        await AddFormats(fmts);

        Formats = fmts;
        SelectedFormat = fmts.First();
    }

    protected virtual async ValueTask AddFormats(List<StreamDisplayFormat> fmts)
    {
        fmts.Add(new StreamDisplayFormat("Disk Representation",
            p => LoadBytesAsync(p, StreamFormat.DiskRepresentation)));
        fmts.Add(new StreamDisplayFormat("Implicit Encryption",
            p => LoadBytesAsync(p, StreamFormat.ImplicitEncryption)));
        var fmtList = (await Source.GetOrNullAsync(KnownNames.Filter)).ObjectAsUnresolvedList();
        for (int i = 0; i < fmtList.Count; i++)
        {
            fmts.Add(new StreamDisplayFormat(fmtList[i].ToString() ?? "No Name",
                p => LoadBytesAsync(p, (StreamFormat)i)));
        }
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
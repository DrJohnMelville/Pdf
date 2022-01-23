using Melville.Linq;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.WpfViewerParts.LowLevelViewer.DocumentParts.Streams;

public class ByteStringViewModel
{
    public ByteStringViewModel(byte[] bytes)
    {
        Bytes = bytes;
    }

    public byte[] Bytes { get; }

    public string HexDump =>
        string.Join("\r\n", Bytes.Take(10240).BinaryFormat().Select((hexDump, index) => $"{index:X7}0  {hexDump}"));

    public string AsAsciiString => ExtendedAsciiEncoding.ExtendedAsciiString(
        TruncatedBytesSpan());

    private Span<byte> TruncatedBytesSpan()
    {
        return Bytes.AsSpan(0, Math.Min(Bytes.Length, 10240));
    }

    public async Task SaveStreamToFile([FromServices] IOpenSaveFile fileDlg)
    {
        var target = fileDlg.GetSaveFile(null, "", "All Files (*.*)|*.*", "Write stream to file");
        if (target == null) return;
        await using var targetStream = await target.CreateWrite();
        await targetStream.WriteAsync(Bytes.AsMemory());
    }

}
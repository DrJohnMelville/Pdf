using System.IO;
using System.Xml;
using System.Xml.Linq;
using Melville.Linq;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.ColorSpaces;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;

public class ByteStringViewModel
{
    public byte[] Bytes { get; }
    public XElement[]? XmlRepresentation { get; }

    public ByteStringViewModel(byte[] bytes)
    {
        Bytes = bytes;
        XmlRepresentation = ParseXml(bytes);
    }

    private XElement[]? ParseXml(byte[] bytes)
    {
        try
        {
            return new []{XElement.Load(new MemoryStream(bytes))};
        }
        catch (Exception)
        {
        }

        return null;
    }

    public string HexDump =>
        string.Join("\r\n", Bytes.BinaryFormat().Select((hexDump, index) => $"{index:X7}0  {hexDump}"));

    public string AsAsciiString => Bytes.ExtendedAsciiString();

    public async Task SaveStreamToFileAsync([FromServices] IOpenSaveFile fileDlg)
    {
        var target = fileDlg.GetSaveFile(null, "", "All Files (*.*)|*.*", "Write stream to file");
        if (target == null) return;
        await using var targetStream = await target.CreateWrite();
        await targetStream.WriteAsync(Bytes.AsMemory());
    }

    public async Task ShowAsIccColorPickerAsync([FromServices] IMvvmDialog dlg) =>
        dlg.ShowPopupWindow(
            await ColorSpaceViewModelFactory.CreateAsync(new MemoryStream(Bytes)),
            800, 400, "Color Picker");
}
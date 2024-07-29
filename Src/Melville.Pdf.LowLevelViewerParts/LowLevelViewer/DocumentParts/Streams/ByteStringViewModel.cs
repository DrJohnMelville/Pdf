using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Melville.Fonts.Type1TextParsers.EexecDecoding;
using Melville.INPC;
using Melville.Linq;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.ColorSpaces;
using Melville.Pdf.LowLevelViewerParts.PostscriptDebuggers;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;


public readonly partial struct XmlRepresentationNode
{
    [FromConstructor] private readonly XElement elt;

    public string Name => $"{elt.Name.LocalName} ({string.Join(", ", Attributes)}) {Content}";
    public IEnumerable<string> Attributes => elt.Attributes().Select(i => $"{i.Name}: {i.Value}");
    public IEnumerable<XmlRepresentationNode> Nodes => elt.Elements().Select(i => new XmlRepresentationNode(i));
    public string Content => elt.Nodes().OfType<XText>().Select(i=>i.Value).ConcatenateStrings();
}

public partial class ByteStringViewModel
{
    [AutoNotify] private byte[] bytes;
    public XmlRepresentationNode[]? XmlRepresentation { get; }
    public string Title => "Raw";

    public ByteStringViewModel(byte[] bytes)
    {
        this.bytes = bytes;
        XmlRepresentation = ParseXml(bytes);
    }

    private XmlRepresentationNode[]? ParseXml(byte[] bytes)
    {
        try
        {
            if (bytes[0] != '<') return null;
            return new []{new XmlRepresentationNode(XElement.Load(new MemoryStream(bytes)))};
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

    [GeneratedRegex(@"\seexec\s+(.*)", RegexOptions.Singleline)]
    private static partial Regex FindEexecPath();

    public void EexecDecode()
    {
        var replstring = FindEexecPath().Replace(AsAsciiString, DecodeString);
        Bytes = replstring.AsExtendedAsciiBytes();
    }

    private string DecodeString(Match match)
    {
        var span = bytes.AsSpan(match.Groups[1].Index);
        // code is 55665
        var decoded = DecodeType1Encoding.DecodeSpan(span, 55665, 4);
        return " eexec \r\n" + decoded.ExtendedAsciiString();
    }

    public void DebugPostscript([FromServices] IMvvmDialog dlg)
    {
        dlg.ShowPopupWindow(new PostscriptDebuggerViewModel(AsAsciiString),
            800, 400, "Postscript Debugger");
    }
}
using System;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public readonly struct UnparsedDictionary
{
    public Memory<byte> Text { get; }

    public UnparsedDictionary(Memory<byte> text)
    {
        Text = text;
    }
    public UnparsedDictionary(string s): this(s.AsExtendedAsciiBytes()){}
}
public interface IMarkedContentCSOperations
{
    /// <summary>
    /// Content stream operator tag MP
    /// </summary>
    void MarkedContentPoint(PdfName tag);

    /// <summary>
    /// Content stream operator tag properties MP
    /// </summary>
    void MarkedContentPoint(PdfName tag, PdfName properties);

    /// <summary>
    /// Content stream operator tag dictionaru MP
    /// </summary>
    void MarkedContentPoint(PdfName tag, in UnparsedDictionary dict);
}
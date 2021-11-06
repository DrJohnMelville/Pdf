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

    /// <summary>
    /// Content stream operator tag BMC
    /// </summary>
    void BeginMarkedRange(PdfName tag);

    /// <summary>
    /// Content stream operator tag dictName BDC
    /// </summary>
    void BeginMarkedRange(PdfName tag, PdfName dictName);

    /// <summary>
    /// Content stream operator tag  inlineDicitionary BDC
    /// </summary>
    void BeginMarkedRange(PdfName tag, in UnparsedDictionary dictionary);

    /// <summary>
    /// Content stream operator EMC
    /// </summary>
    void EndMarkedRange();
}
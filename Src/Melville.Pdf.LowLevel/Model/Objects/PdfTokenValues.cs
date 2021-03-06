using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public class PdfTokenValues: PdfObject
{
    public byte[] TokenValue { get; }

    protected PdfTokenValues(byte[] tokenValue)
    {
        TokenValue = tokenValue;
    }

    public override string ToString() => ExtendedAsciiEncoding.ExtendedAsciiString(TokenValue);

    public static readonly PdfTokenValues Null = new (new byte[]{110, 117, 108, 108}); // null
    // These are not part of the PDF spec -- they are sentinels for a parser implementation trick;
    public static readonly PdfTokenValues ArrayTerminator = new(new byte[]{93}); // ]
    public static readonly PdfTokenValues DictionaryTerminator = new(new byte[]{62,62});//>>
    public static readonly PdfTokenValues InlineImageDictionaryTerminator =
        new(new byte[] { (byte)'I', (byte)'D' });
    public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
}
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public class PdfTokenValues: PdfObject
{
    internal byte[] TokenValue { get; }

    private protected PdfTokenValues(byte[] tokenValue)
    {
        TokenValue = tokenValue;
    }

    /// <inheritdoc />
    public override string ToString() => ExtendedAsciiEncoding.ExtendedAsciiString(TokenValue);
    
    /// <summary>
    /// This flyweight class represents the PDF null object.
    /// </summary>
    public static readonly PdfTokenValues Null = new (new byte[]{110, 117, 108, 108}); // null

    // These are not part of the PDF spec -- they are sentinels for a parser implementation trick;
    internal static readonly PdfTokenValues ArrayTerminator = new(new byte[]{93}); // ]
    internal static readonly PdfTokenValues DictionaryTerminator = new(new byte[]{62,62});//>>
    internal static readonly PdfTokenValues InlineImageDictionaryTerminator =
        new(new byte[] { (byte)'I', (byte)'D' });
    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
}
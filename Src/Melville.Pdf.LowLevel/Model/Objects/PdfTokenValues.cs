using System;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.ShortStrings;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// These are PDF objests who's identity is their value, including Null and some
/// sentinel pseudo values that are used in the parser.  True and false are also
/// token values, but they descend through the child class PdfBoolean
/// </summary>
public class PdfTokenValues: PdfObject
{
#if false
    internal byte[] TokenValue { get; }

    private protected PdfTokenValues(in ReadOnlySpan<byte> tokenValue)
    {
        TokenValue = tokenValue.ToArray();
    }
    
    /// <inheritdoc />
    public override string ToString() => ExtendedAsciiEncoding.ExtendedAsciiString(TokenValue);
#else
    internal ShortString<NinePackedBytes> TokenValue { get; }

    private protected PdfTokenValues(in ReadOnlySpan<byte> tokenValue)
    {
        TokenValue = new ShortString<NinePackedBytes>(new(tokenValue));
    }

    /// <inheritdoc />
    public override string ToString() => TokenValue.ValueAsString();
#endif
    /// <summary>
    /// This flyweight class represents the PDF null object.
    /// </summary>
    public static readonly PdfTokenValues Null = new ("null"u8); // null

    // These are not part of the PDF spec -- they are sentinels for a parser implementation trick;
    internal static readonly PdfTokenValues ArrayTerminator = new("]"u8); // ]
    internal static readonly PdfTokenValues DictionaryTerminator = new(">>"u8);//>>
    internal static readonly PdfTokenValues InlineImageDictionaryTerminator =
        new("ID"u8);
    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
}
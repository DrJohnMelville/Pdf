using System;
using System.Collections.Generic;
using System.Diagnostics;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// A PdfString is a sequence of bytes -- the interpretation of those bytes depends
/// (a lot) on the context that string is in.  PdfString has a lot of methods to get data
/// into and out of PdfStrings
/// </summary>
[DebuggerDisplay("PdfString ({ToString()}) <{Bytes.AsHex()}>")]
public sealed class PdfString : PdfByteArrayObject, IComparable<PdfString>
{
    /// <summary>
    /// An empty PdfString
    /// </summary>
    public static readonly PdfString Empty = new PdfString(Array.Empty<byte>());

    /// <summary>
    /// Create a PDF string from a byte array.  The string keeps the byte array and assumes the caller will not touch it again.
    /// </summary>
    /// <param name="bytes">The bytes that make up the new PdfString</param>
    public PdfString(byte[] bytes): base(bytes) { }


    /// <inheritdoc />
    public override string ToString() => Bytes.PdfDocEncodedString();

    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    /// <summary>
    /// Create a PdfString from a C# string using the bottom 8 bits of each character.
    /// </summary>
    /// <param name="str">The string to represent as a PdfString</param>
    /// <returns>The created PdfString</returns>
    public static PdfString CreateAscii(string str) => new(str.AsExtendedAsciiBytes());
    /// <summary>
    /// Get the value of a PdfString as a C# string maping each ascii character to the same unicode code point.
    /// </summary>
    /// <returns>The value as a string/</returns>
    public String AsAsciiString() => Bytes.PdfDocEncodedString();

    /// <summary>
    /// Create a PDF string ecoding a C# string using UTF-16 BE.
    /// </summary>
    /// <param name="text">The string to encode as a UTF-16 PdfString</param>
    /// <returns>The resulting string.</returns>
    public static PdfString CreateUtf16(string text) => new(
        UnicodeEncoder.BigEndian.GetBytesWithBOM(text));
    /// <summary>
    /// Decode a PdfString into a C# string using UTF-16 BE
    /// </summary>
    /// <returns>The decoded C# string</returns>
    public string AsUtf16() => UnicodeEncoder.BigEndian.GetString(Bytes);

    /// <summary>
    /// Create a PdfString from a C# string using PDF Default Encoding
    /// </summary>
    /// <param name="text">The string value to encode</param>
    /// <returns>The resulting PdfString</returns>
    public static PdfString CreatePdfEncoding(string text) => new(text.AsPdfDocBytes());
    /// <summary>
    /// Decode a PdfString to a C# string using PdfDocEncoding
    /// </summary>
    /// <returns>The decoded c# string</returns>
    public string AsPdfDocEnccodedString() => Bytes.PdfDocEncodedString();

    /// <summary>
    /// Decode a PdfString to a C# string, using byte order marks to dictate the decoding method;
    /// </summary>
    /// <returns></returns>
    public string AsTextString()
    {
        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(Bytes);
        return UnicodeEncoder.BigEndian.TryGetFromBOM(span) ??
               UnicodeEncoder.LittleEndian.TryGetFromBOM(span) ??
               UnicodeEncoder.Utf8.TryGetFromBOM(span)??
               AsPdfDocEnccodedString();
    }

    /// <summary>
    /// Parse a PdfString that represents a DateTime field in PDF format/
    /// </summary>
    /// <returns>The date /time as a PdfTime structure</returns>
    public PdfTime AsPdfTime() => new PdfTimeParser(AsTextString().AsSpan()).AsPdfTime();
    /// <summary>
    /// Format a PdfTime into a string for storage in a PDF file.
    /// </summary>
    /// <param name="time">The Date and Time to be encoded/</param>
    /// <returns></returns>
    public static PdfString CreateDate(PdfTime time) => new PdfString(time.AsPdfBytes());

    /// <inheritdoc />
    public int CompareTo(PdfString? other) => 
        other == null ? 1 : Bytes.AsSpan().SequenceCompareTo(other.Bytes);

    /// <summary>
    /// Encode a c# string as a PDF string using UTF-8 encoding.
    /// </summary>
    /// <param name="text">The text to encode</param>
    /// <returns>The encoded PDF string/</returns>
    public static PdfString CreateUtf8(string text) => 
        new(UnicodeEncoder.Utf8.GetBytesWithBOM(text));

    /// <summary>
    /// Decode a utf-8 encoded PDFString to a c# string.
    /// </summary>
    /// <returns>The decoded c# string/</returns>
    public string AsUtf8() => UnicodeEncoder.Utf8.GetString(Bytes);

    /// <summary>
    /// Create a PdfString from a C# string
    /// </summary>
    /// <param name="value">The desired C# value</param>
    public static implicit operator PdfString(string value) => PdfString.CreateAscii(value);
}
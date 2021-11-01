using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public sealed class PdfBoolean : PdfTokenValues
{
    public bool Value => this == True;
    public static readonly PdfBoolean True = new(new byte[]{116, 114, 117, 101}); // true
    public static readonly PdfBoolean False = new(new byte[]{102, 97, 108, 115, 101}); // false
    public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    public PdfBoolean(byte[] tokenValue) : base(tokenValue)
    {
    }
}
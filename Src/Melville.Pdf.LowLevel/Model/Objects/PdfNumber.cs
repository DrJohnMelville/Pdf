using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public abstract class PdfNumber: PdfObject
    {
        public abstract long IntValue { get; }
        public abstract double DoubleValue { get; }
    }

    public sealed class PdfInteger : PdfNumber
    {
        public override long IntValue { get; }
        public override double DoubleValue => IntValue;
        public PdfInteger(long value)
        {
            IntValue = value;
        }
        public override string ToString() => IntValue.ToString();
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
    }
    public sealed class PdfDouble : PdfNumber
    {
        public override long IntValue => (long) DoubleValue;
        public override double DoubleValue { get; }
        public PdfDouble(double value)
        {
            DoubleValue = value;
        }

        public override string ToString() => DoubleValue.ToString();
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
    }
}
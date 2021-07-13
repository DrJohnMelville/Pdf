namespace Melville.Pdf.LowLevel.Model.Objects
{
    public abstract class PdfNumber: PdfObject
    {
        public abstract int IntValue { get; }
        public abstract double DoubleValue { get; }
    }

    public sealed class PdfInteger : PdfNumber
    {
        public override int IntValue { get; }
        public override double DoubleValue => IntValue;
        public PdfInteger(int value)
        {
            IntValue = value;
        }
        public override string ToString() => IntValue.ToString();
    }
    public sealed class PdfDouble : PdfNumber
    {
        public override int IntValue => (int) DoubleValue;
        public override double DoubleValue { get; }
        public PdfDouble(double value)
        {
            DoubleValue = value;
        }

        public override string ToString() => DoubleValue.ToString();
    }
}
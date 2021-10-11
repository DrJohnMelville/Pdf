using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions
{
    public struct ClosedInterval
    {
        private double minValue;
        private double maxValue;

        public ClosedInterval(double minValue, double maxValue)
        {
            if(minValue > maxValue) 
                throw new PdfParseException("Empty Interval");
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public double Clip(double val) =>
            val > maxValue ? maxValue :
            val < minValue ? minValue : val;

        public bool OutOfInterval(double value) => value < minValue || value > maxValue;

        public static readonly ClosedInterval NoRestriction = new(double.MinValue, double.MaxValue);
    }
}
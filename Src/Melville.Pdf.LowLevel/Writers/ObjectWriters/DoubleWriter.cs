using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters
{
    public static class DoubleWriter
    {
        public static ValueTask<FlushResult> Write(PipeWriter target, double item)
        {
            var span = target.GetSpan(25);
            var written = Write(item, span);
            target.Advance(written);
            return target.FlushAsync();
        }

        public static int Write(double item, in Span<byte> span)
        {
            return new DoubleSpanWriter(span).Write(item);
        }
    }

    public ref struct DoubleSpanWriter
    {
        private Span<byte> span;
        private int length;
        private double frac;

        public DoubleSpanWriter(Span<byte> span) : this()
        {
            this.span = span;
        }

        public int Write(double item)
        {
            var truncated = Math.Truncate(item);
            length = IntegerWriter.Write(span, (long)truncated);
            frac = Math.Abs(item - truncated);
            if (!ShouldWriteMoreFractionalDigits()) return length;
            WriteFractionalPart();
            FixupAfterPrinting();
            return length;
        }
        
        private bool ShouldWriteMoreFractionalDigits() =>
            LengthLessThanDoublePrecision() && length < span.Length && frac > double.Epsilon;

        // doubles have 16 digits of precision so unless we need it for magnitude reasons it never
        // makes sense to print more that 16 digits + 1 character for the decimal point because we
        // would just be printing random number anyway.  Technically this means we lose the 16th
        // significant digit on negative numbers, but I am ok with that.
        // we also lose precision on numbers very close to zero.  I'm ok with that too.
        private bool LengthLessThanDoublePrecision() => length < 17;

        private void WriteFractionalPart()
        {
            span[length++] = (byte)'.';
            while (ShouldWriteMoreFractionalDigits())
            {
                frac *= 10;
                var digit = Math.Truncate(frac);
                frac -= digit;
                span[length++] = (byte)('0' + (int)digit);
            }
        }
        private void FixupAfterPrinting()
        {
            if (frac >= 0.5) 
                RoundUp();
            else
                PruneZeros();
        }
        
        private void RoundUp()
        {
            while (true)
            {
                switch ((char)span[length - 1])
                {
                    case '.':
                        length--;
                        RoundUpWholeNumber();
                        return;
                    case '9':
                        length--;
                        break;
                    default:
                        span[length - 1]++;
                        return;
                }
            }
        }

        private void RoundUpWholeNumber()
        {
            int currentDigit = length - 1;
            while (true)
            {
                if (RoundUpPastFirstDigit(currentDigit))
                {
                    PrependOneToNumber(currentDigit);
                    return;
                }
                if ((char)span[currentDigit] != '9')
                {
                    span[currentDigit]++;
                    return;
                }
                span[currentDigit] = (byte)'0';
                currentDigit--;
            }
        }

        private void PrependOneToNumber(int currentDigit)
        {
            length++;
            ShiftSpanRight(span.Slice(currentDigit + 1, length));
            span[currentDigit + 1] = (byte)'1';
        }

        private bool RoundUpPastFirstDigit(int currentDigit) => 
            currentDigit < 0 || span[currentDigit] == (byte)'-';

        private void ShiftSpanRight(in Span<byte> slice) => slice[..^1].CopyTo(slice[1..]);

        private void PruneZeros()
        {
            while (length > 1)
            {
                switch ((char)span[length - 1])
                {
                    case '.': 
                        length--;
                        return;
                    case '0': length--;
                        break;
                    default: return;
                }
            }
        }
    }
}
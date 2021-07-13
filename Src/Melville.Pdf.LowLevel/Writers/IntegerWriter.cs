using System;
using System.IO.Pipelines;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers
{
    public static class IntegerWriter
    {
        public static ValueTask<FlushResult> Write(PipeWriter target, int item)
        {
            var buffer = target.GetSpan(12);
            target.Advance(CopyNumberToBuffer(buffer, item));
            return target.FlushAsync();
        }

        public static int CopyNumberToBuffer(Span<byte> buffer, int item)
        {
            if (item < 0)
            {
                buffer[0] = (byte) '-';
                return WritePositiveNumber(buffer.Slice(1), (uint) (-item))+1;
            }

            return WritePositiveNumber(buffer, (uint)item);
        }

        private static unsafe int WritePositiveNumber(Span<byte> slice, uint value)
        {
            var digits = CountDigits(value);
            fixed (byte* current = slice)
            {
                byte* curPos = current + digits;
                do
                {
                    value = DivRem(value, 10, out uint remainder);
                    *(--curPos) = (byte) ((byte) '0' + remainder);
                } while (value != 0);
            }

            return digits;
        }
        internal static uint DivRem(uint a, uint b, out uint result)
        {
            uint div = a / b;
            result = a - (div * b);
            return div;
        }

        private static int CountDigits(uint value) => value switch
        {
            >999999999 => 10,
            >99999999 => 9,
            >9999999 => 8,
            >999999 => 7,
            >99999 => 6,
            >9999 => 5,
            >999 => 4,
            >99 => 3,
            >9 => 2,
            _ => 1
        };
    }
}
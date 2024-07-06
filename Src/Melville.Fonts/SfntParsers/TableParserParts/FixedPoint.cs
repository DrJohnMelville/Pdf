using System.Numerics;
using System.Runtime.CompilerServices;
using Melville.INPC;

namespace Melville.Fonts.SfntParsers.TableParserParts;

/// <summary>
/// This interface statically controls where the binary point is in a fixed point number.
/// </summary>
public interface IFixedPointConfig<TStorage, TCompute>
where TStorage: IBinaryInteger<TStorage>
where TCompute: IBinaryInteger<TCompute>
{
    /// <summary>
    /// Number of bits to the right of the binary point
    /// </summary>
    public static abstract int FractionalBits { get; }

    /// <summary>
    /// Convert an extended compute value back to the cannonical representation
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <returns>The passed in value in the storage representation</returns>
    public static abstract TStorage ToStorage(TCompute value);
    /// <summary>
    /// Convert the canonicak representation into a wider format to do math without losing digits.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <returns>The passed in value in the storage representation</returns>
    public static abstract TCompute ToCompute(TStorage value);
}

/// <summary>
/// Fixed number configuration for a fixed point number with a 16 bit fraction
/// </summary>
public readonly struct Fixed16 : IFixedPointConfig<int, long>
{
    /// <inheritdoc />
    public static int FractionalBits => 16;

    /// <inheritdoc />
    public static int ToStorage(long value) => (int)value;  

    /// <inheritdoc />
    public static long ToCompute(int value) => value;
}

/// <summary>
/// Represents a number with a fixed number of bits to the right of the binary point.
/// </summary>
/// <typeparam name="TStorage">The type used to store the value</typeparam>
/// <typeparam name="TCompute">A type (typically twice as wide as storage) used for computation</typeparam>
/// <typeparam name="TConfig">A configuration type that defines the location of the binary point.</typeparam>
/// <param name="value">The value of the FixedPoint, in the storage format.</param>
public readonly struct FixedPoint<TStorage, TCompute,TConfig>(TStorage value) 
    where TStorage: IBinaryInteger<TStorage> 
    where TCompute: IBinaryInteger<TCompute>
    where TConfig: IFixedPointConfig<TStorage, TCompute>
{
    private static TCompute FractionalMask => Const(ScaleFactor - TCompute.One);

    private static TCompute Const<TC>(TC scaleFactor) 
        where TC:IBinaryInteger<TC> => TCompute.CreateTruncating(scaleFactor);

    private static TCompute ScaleFactor => TCompute.One << TConfig.FractionalBits;

    /// <inheritdoc />
    public override string ToString()
    {
        Span<char> buffer = stackalloc char[16];
        return new string(new FixedPointPrinter(TConfig.ToCompute(value), buffer).Print());
    }

    private ref struct FixedPointPrinter
    {
        private TCompute value;
        private Span<char> buffer;
        private int pos;

        public FixedPointPrinter(TCompute value, Span<char> buffer)
        {
            this.value = value;
            this.buffer = buffer;
            pos = 0;
        }

        public Span<char> Print() =>
            value >= TCompute.Zero ? PrintPositiveNumber() : PrintNegativeNumber();

        private Span<char> PrintNegativeNumber()
        {
            buffer[0] = '-';
            var positive = new FixedPointPrinter(-value, buffer[1..]).Print();
            return positive.Length == 0 ? 
                positive : 
                buffer[..(positive.Length+1)];
        }

        private Span<char> PrintPositiveNumber() =>
            PrintWholeNumber() && PrintFractionalPart() ? 
                buffer[..pos] : [];

        private bool PrintWholeNumber() => 
            WholeNumberPart().TryFormat(buffer, out pos, [], null);

        private TCompute WholeNumberPart() => value >> TConfig.FractionalBits;

        private bool PrintFractionalPart() 
        {
            var fractional = value & FractionalMask;
            if (fractional == TCompute.Zero) return true;

            if (pos + 2 > buffer.Length) return false;
            buffer[pos++] = '.';

            PrintFractionDigits(fractional);

            return true;
        }

        private void PrintFractionDigits(TCompute fractional)
        {
            var ten = Const(10);
            while (pos < buffer.Length && fractional > TCompute.Zero)
            {
                fractional *= ten;
                buffer[pos++] = (char)('0' + Convert.ToChar(fractional >> TConfig.FractionalBits));
                fractional &= FractionalMask;
            }
        }
    }
}


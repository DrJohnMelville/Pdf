using System.Buffers;
using System.Runtime.CompilerServices;

namespace Melville.JpegLibrary.Decoder;

public readonly struct JpegFrameComponentSpecificationParameters
{
    public JpegFrameComponentSpecificationParameters(byte identifier, byte horizontalSamplingFactor, byte verticalSamplingFactor, byte quantizationTableSelector)
    {
        Identifier = identifier;
        HorizontalSamplingFactor = horizontalSamplingFactor;
        VerticalSamplingFactor = verticalSamplingFactor;
        QuantizationTableSelector = quantizationTableSelector;
    }

    public byte Identifier { get; }
    public byte HorizontalSamplingFactor { get; }
    public byte VerticalSamplingFactor { get; }
    public byte QuantizationTableSelector { get; }

    [SkipLocalsInit]
    public static bool TryParse(ReadOnlySequence<byte> buffer, out JpegFrameComponentSpecificationParameters component)
    {
        if (buffer.IsSingleSegment)
        {
            return TryParse(buffer.FirstSpan, out component);
        }

        if (buffer.Length < 3)
        {
            component = default;
            return false;
        }

        Span<byte> local = stackalloc byte[3];
        buffer.Slice(0, 3).CopyTo(local);

        byte quantizationTableSelector = local[2];
        byte samplingFactor = local[1];
        byte identifier = local[0];

        component = new JpegFrameComponentSpecificationParameters(identifier, (byte)(samplingFactor >> 4), (byte)(samplingFactor & 0xf), quantizationTableSelector);
        return true;
    }

    public static bool TryParse(ReadOnlySpan<byte> buffer, out JpegFrameComponentSpecificationParameters component)
    {
        if (buffer.Length < 3)
        {
            component = default;
            return false;
        }

        byte quantizationTableSelector = buffer[2];
        byte samplingFactor = buffer[1];
        byte identifier = buffer[0];

        component = new JpegFrameComponentSpecificationParameters(identifier, (byte)(samplingFactor >> 4), (byte)(samplingFactor & 0xf), quantizationTableSelector);
        return true;
    }
}
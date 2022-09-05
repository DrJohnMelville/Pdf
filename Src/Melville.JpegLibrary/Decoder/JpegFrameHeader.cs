using System.Buffers;
using System.Runtime.CompilerServices;

namespace Melville.JpegLibrary.Decoder;

public readonly struct JpegFrameHeader
{
    public JpegFrameHeader(byte samplePrecision, ushort numberOfLines, ushort samplesPerLine, byte numberOfComponents, JpegFrameComponentSpecificationParameters[]? components)
    {
        SamplePrecision = samplePrecision;
        NumberOfLines = numberOfLines;
        SamplesPerLine = samplesPerLine;
        NumberOfComponents = numberOfComponents;
        Components = components;
    }

    public JpegFrameComponentSpecificationParameters[]? Components { get; }

    public byte SamplePrecision { get; }
    public ushort NumberOfLines { get; }
    public ushort SamplesPerLine { get; }
    public byte NumberOfComponents { get; }
    public byte BytesRequired => (byte)(6 + 3 * NumberOfComponents);

    [SkipLocalsInit]
    public static bool TryParse(ReadOnlySequence<byte> buffer, bool metadataOnly, out JpegFrameHeader frameHeader, out int bytesConsumed)
    {
        if (buffer.IsSingleSegment)
        {
            return TryParse(buffer.FirstSpan, metadataOnly, out frameHeader, out bytesConsumed);
        }

        bytesConsumed = 0;

        if (buffer.Length < 6)
        {
            frameHeader = default;
            return false;
        }

        Span<byte> local = stackalloc byte[6];
        buffer.Slice(0, 6).CopyTo(local);

        byte numberOfComponenets = local[5];
        ushort samplesPerLine = (ushort)(local[4] | (local[3] << 8));
        ushort numberOfLines = (ushort)(local[2] | (local[1] << 8));
        byte precision = local[0];

        buffer = buffer.Slice(6);
        bytesConsumed += 6;

        if (buffer.Length < 3 * numberOfComponenets)
        {
            frameHeader = default;
            return false;
        }

        if (metadataOnly)
        {
            bytesConsumed += 3 * numberOfComponenets;
            frameHeader = new JpegFrameHeader(precision, numberOfLines, samplesPerLine, numberOfComponenets, null);
            return true;
        }

        JpegFrameComponentSpecificationParameters[] components = new JpegFrameComponentSpecificationParameters[numberOfComponenets];
        for (int i = 0; i < components.Length; i++)
        {
            if (!JpegFrameComponentSpecificationParameters.TryParse(buffer, components.Length, out components[i]))
            {
                frameHeader = default;
                return false;
            }
            buffer = buffer.Slice(3);
            bytesConsumed += 3;
        }

        frameHeader = new JpegFrameHeader(precision, numberOfLines, samplesPerLine, numberOfComponenets, components);
        return true;
    }

    public static bool TryParse(ReadOnlySpan<byte> buffer, bool metadataOnly, out JpegFrameHeader frameHeader, out int bytesConsumed)
    {
        bytesConsumed = 0;

        if (buffer.Length < 6)
        {
            frameHeader = default;
            return false;
        }

        byte numberOfComponenets = buffer[5];
        ushort samplesPerLine = (ushort)(buffer[4] | (buffer[3] << 8));
        ushort numberOfLines = (ushort)(buffer[2] | (buffer[1] << 8));
        byte precision = buffer[0];

        buffer = buffer.Slice(6);
        bytesConsumed += 6;

        if (buffer.Length < 3 * numberOfComponenets)
        {
            frameHeader = default;
            return false;
        }

        if (metadataOnly)
        {
            bytesConsumed += 3 * numberOfComponenets;
            frameHeader = new JpegFrameHeader(precision, numberOfLines, samplesPerLine, numberOfComponenets, null);
            return true;
        }

        JpegFrameComponentSpecificationParameters[] components = new JpegFrameComponentSpecificationParameters[numberOfComponenets];
        for (int i = 0; i < components.Length; i++)
        {
            if (!JpegFrameComponentSpecificationParameters.TryParse(buffer, components.Length, out components[i]))
            {
                frameHeader = default;
                return false;
            }
            buffer = buffer.Slice(3);
            bytesConsumed += 3;
        }

        frameHeader = new JpegFrameHeader(precision, numberOfLines, samplesPerLine, numberOfComponenets, components);
        return true;
    }
    internal bool ShadowEquals(JpegFrameHeader other)
    {
        return SamplePrecision == other.SamplePrecision && NumberOfLines == other.NumberOfLines && SamplesPerLine == other.SamplesPerLine && NumberOfComponents == other.NumberOfComponents;
    }
}
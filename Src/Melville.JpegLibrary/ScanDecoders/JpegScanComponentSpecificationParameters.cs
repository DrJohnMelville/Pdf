using System.Buffers;
using System.Runtime.CompilerServices;

namespace Melville.JpegLibrary.ScanDecoders;

public readonly struct JpegScanComponentSpecificationParameters
{
    public JpegScanComponentSpecificationParameters(byte scanComponentSelector, byte dcEntropyCodingTableSelector, byte acEntropyCodingTableSelector)
    {
        ScanComponentSelector = scanComponentSelector;
        DcEntropyCodingTableSelector = dcEntropyCodingTableSelector;
        AcEntropyCodingTableSelector = acEntropyCodingTableSelector;
    }

    public byte ScanComponentSelector { get; }
    public byte DcEntropyCodingTableSelector { get; }
    public byte AcEntropyCodingTableSelector { get; }

    [SkipLocalsInit]
    public static bool TryParse(ReadOnlySequence<byte> buffer, out JpegScanComponentSpecificationParameters component)
    {
        if (buffer.IsSingleSegment)
        {
            return TryParse(buffer.FirstSpan, out component);
        }

        if (buffer.Length < 2)
        {
            component = default;
            return false;
        }

        Span<byte> local = stackalloc byte[2];
        buffer.Slice(0, 2).CopyTo(local);

        byte entropyCodingTableSelector = local[1];
        byte scanComponentSelector = local[0];

        component = new JpegScanComponentSpecificationParameters(scanComponentSelector, (byte)(entropyCodingTableSelector >> 4), (byte)(entropyCodingTableSelector & 0xf));
        return true;
    }

    public static bool TryParse(ReadOnlySpan<byte> buffer, out JpegScanComponentSpecificationParameters component)
    {
        if (buffer.Length < 2)
        {
            component = default;
            return false;
        }

        byte entropyCodingTableSelector = buffer[1];
        byte scanComponentSelector = buffer[0];

        component = new JpegScanComponentSpecificationParameters(scanComponentSelector, (byte)(entropyCodingTableSelector >> 4), (byte)(entropyCodingTableSelector & 0xf));
        return true;
    }

}
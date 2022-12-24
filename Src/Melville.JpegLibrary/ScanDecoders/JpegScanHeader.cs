using System.Buffers;
using System.Runtime.CompilerServices;

namespace Melville.JpegLibrary.ScanDecoders;

internal readonly struct JpegScanHeader
{
    public JpegScanHeader(byte numberOfComponents, JpegScanComponentSpecificationParameters[]? components, byte startOfSpectralSelection, byte endOfSpectralSelection, byte successiveApproximationBitPositionHigh, byte successiveApproximationBitPositionLow)
    {
        NumberOfComponents = numberOfComponents;
        Components = components;
        StartOfSpectralSelection = startOfSpectralSelection;
        EndOfSpectralSelection = endOfSpectralSelection;
        SuccessiveApproximationBitPositionHigh = successiveApproximationBitPositionHigh;
        SuccessiveApproximationBitPositionLow = successiveApproximationBitPositionLow;
    }

    /// <summary>
    /// Parameters for each component.
    /// </summary>
    public JpegScanComponentSpecificationParameters[]? Components { get; }

    /// <summary>
    /// The number of component in this scan.
    /// </summary>
    public byte NumberOfComponents { get; }

    /// <summary>
    /// Start of spectral selection.
    /// </summary>
    public byte StartOfSpectralSelection { get; }

    /// <summary>
    /// End of spectral selection.
    /// </summary>
    public byte EndOfSpectralSelection { get; }

    /// <summary>
    /// Successive approximation bit position (high).
    /// </summary>
    public byte SuccessiveApproximationBitPositionHigh { get; }

    /// <summary>
    /// Successive approximation bit position (low).
    /// </summary>
    public byte SuccessiveApproximationBitPositionLow { get; }

    /// <summary>
    /// Gets the count of bytes required to encode this scan header.
    /// </summary>
    public byte BytesRequired => (byte)(4 + 2 * NumberOfComponents);

    internal bool ShadowEquals(JpegScanHeader other)
    {
        return NumberOfComponents == other.NumberOfComponents && StartOfSpectralSelection == other.StartOfSpectralSelection && EndOfSpectralSelection == other.EndOfSpectralSelection &&
               SuccessiveApproximationBitPositionHigh == other.SuccessiveApproximationBitPositionHigh && SuccessiveApproximationBitPositionLow == other.SuccessiveApproximationBitPositionLow;
    }

    /// <summary>
    /// Parse the scan header from the buffer.
    /// </summary>
    /// <param name="buffer">The buffer to read from.</param>
    /// <param name="metadataOnly">True if the construction of the <see cref="JpegScanComponentSpecificationParameters"/> array should be suppressed.</param>
    /// <param name="scanHeader">The scan header parsed.</param>
    /// <param name="bytesConsumed">The count of bytes consumed by the parser.</param>
    /// <returns>True is the scan header is successfully parsed.</returns>
    [SkipLocalsInit]
    public static bool TryParse(ReadOnlySequence<byte> buffer, bool metadataOnly, out JpegScanHeader scanHeader, out int bytesConsumed)
    {
        if (buffer.IsSingleSegment)
        {
            return TryParse(buffer.FirstSpan, metadataOnly, out scanHeader, out bytesConsumed);
        }

        bytesConsumed = 0;

        if (buffer.IsEmpty)
        {
            scanHeader = default;
            return false;
        }

        byte numberOfComponents = buffer.FirstSpan[0];
        buffer = buffer.Slice(1);
        bytesConsumed++;

        if (buffer.Length < (2 * numberOfComponents + 3))
        {
            scanHeader = default;
            return false;
        }

        JpegScanComponentSpecificationParameters[]? components;
        if (metadataOnly)
        {
            components = null;
            buffer = buffer.Slice(2 * numberOfComponents);
            bytesConsumed += 2 * numberOfComponents;
        }
        else
        {
            components = new JpegScanComponentSpecificationParameters[numberOfComponents];
            for (int i = 0; i < components.Length; i++)
            {
#pragma warning disable CA1806
                JpegScanComponentSpecificationParameters.TryParse(buffer, out components[i]);
#pragma warning restore CA1806 
                buffer = buffer.Slice(2);
                bytesConsumed += 2;
            }
        }

        Span<byte> local = stackalloc byte[4];
        buffer.Slice(0, 3).CopyTo(local);

        byte successiveApproximationBitPosition = local[2];
        byte endOfSpectralSelection = local[1];
        byte startOfSpectralSelection = local[0];
        bytesConsumed += 3;

        scanHeader = new JpegScanHeader(numberOfComponents, components, startOfSpectralSelection, endOfSpectralSelection, (byte)(successiveApproximationBitPosition >> 4), (byte)(successiveApproximationBitPosition & 0xf));
        return true;

    }

    public static bool TryParse(ReadOnlySpan<byte> buffer, bool metadataOnly, out JpegScanHeader scanHeader, out int bytesConsumed)
    {
        bytesConsumed = 0;

        if (buffer.IsEmpty)
        {
            scanHeader = default;
            return false;
        }

        byte numberOfComponents = buffer[0];
        buffer = buffer.Slice(1);
        bytesConsumed++;

        if (buffer.Length < (2 * numberOfComponents + 3))
        {
            scanHeader = default;
            return false;
        }

        JpegScanComponentSpecificationParameters[]? components;
        if (metadataOnly)
        {
            components = null;
            buffer = buffer.Slice(2 * numberOfComponents);
            bytesConsumed += 2 * numberOfComponents;
        }
        else
        {
            components = new JpegScanComponentSpecificationParameters[numberOfComponents];
            for (int i = 0; i < components.Length; i++)
            {
#pragma warning disable CA1806
                JpegScanComponentSpecificationParameters.TryParse(buffer, out components[i]);
#pragma warning restore CA1806
                buffer = buffer.Slice(2);
                bytesConsumed += 2;
            }
        }

        byte successiveApproximationBitPosition = buffer[2];
        byte endOfSpectralSelection = buffer[1];
        byte startOfSpectralSelection = buffer[0];
        bytesConsumed += 3;

        scanHeader = new JpegScanHeader(numberOfComponents, components, startOfSpectralSelection, endOfSpectralSelection, (byte)(successiveApproximationBitPosition >> 4), (byte)(successiveApproximationBitPosition & 0xf));
        return true;
    }
}
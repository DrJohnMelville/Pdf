using System.Runtime.CompilerServices;
using Melville.JpegLibrary.ArithmeticDecoding;
using Melville.JpegLibrary.BlockOutputWriters;
using Melville.JpegLibrary.Decoder;
using Melville.JpegLibrary.Maths;
using Melville.JpegLibrary.Readers;

namespace Melville.JpegLibrary.ScanDecoders;

internal class JpegArithmeticSequentialScanDecoder : JpegArithmeticScanDecoder
{
    private readonly JpegFrameHeader _frameHeader;

    private readonly int _maxHorizontalSampling;
    private readonly int _maxVerticalSampling;

    private readonly ushort _restartInterval;
    private readonly int _mcusPerLine;
    private readonly int _mcusPerColumn;
    private readonly int _levelShift;

    private readonly JpegArithmeticDecodingComponent[] _components;

    public JpegArithmeticSequentialScanDecoder(JpegDecoder decoder, JpegFrameHeader frameHeader) : base(decoder)
    {
        _frameHeader = frameHeader;

        // Compute maximum sampling factor
        int maxHorizontalSampling = 1;
        int maxVerticalSampling = 1;
        foreach (JpegFrameComponentSpecificationParameters currentFrameComponent in frameHeader.Components!)
        {
            maxHorizontalSampling = Math.Max(maxHorizontalSampling, currentFrameComponent.HorizontalSamplingFactor);
            maxVerticalSampling = Math.Max(maxVerticalSampling, currentFrameComponent.VerticalSamplingFactor);
        }
        _maxHorizontalSampling = maxHorizontalSampling;
        _maxVerticalSampling = maxVerticalSampling;

        _restartInterval = decoder.GetRestartInterval();
        _mcusPerLine = (frameHeader.SamplesPerLine + 8 * maxHorizontalSampling - 1) / (8 * maxHorizontalSampling);
        _mcusPerColumn = (frameHeader.NumberOfLines + 8 * maxVerticalSampling - 1) / (8 * maxVerticalSampling);
        _levelShift = 1 << (frameHeader.SamplePrecision - 1);

        // Pre-allocate the JpegDecodeComponent instances
        _components = new JpegArithmeticDecodingComponent[frameHeader.NumberOfComponents];
        for (int i = 0; i < _components.Length; i++)
        {
            _components[i] = new JpegArithmeticDecodingComponent();
        }
    }

    public override void ProcessScan(ref JpegReader reader, JpegScanHeader scanHeader)
    {
        JpegFrameHeader frameHeader = _frameHeader;
        JpegBlockOutputWriter? outputWriter = Decoder.GetOutputWriter();

        if (frameHeader.Components is null)
        {
            ThrowInvalidDataException("Component parameters are missing in JPEG frame header.");
        }
        if (scanHeader.Components is null)
        {
            ThrowInvalidDataException("Component parameters are missing in JPEG scan header.");
        }
        if (outputWriter is null)
        {
            throw new InvalidOperationException("Output writer is not specified.");
        }

        // Resolve each component
        Span<JpegArithmeticDecodingComponent> components = _components.AsSpan(0, InitDecodeComponents(frameHeader, scanHeader, _components));

        foreach (JpegArithmeticDecodingComponent component in _components)
        {
            component.DcPredictor = 0;
            component.DcContext = 0;
            component.DcStatistics?.Reset();
            component.AcStatistics?.Reset();
        }

        Reset();

        // Prepare
        int maxHorizontalSampling = _maxHorizontalSampling;
        int maxVerticalSampling = _maxVerticalSampling;
        int mcusBeforeRestart = _restartInterval;
        int mcusPerLine = _mcusPerLine;
        int mcusPerColumn = _mcusPerColumn;
        int levelShift = _levelShift;
        JpegBitReader bitReader = new JpegBitReader(reader.RemainingBytes);

        // DCT Block
        Unsafe.SkipInit(out JpegBlock8x8F blockFBuffer);
        Unsafe.SkipInit(out JpegBlock8x8F outputFBuffer);
        Unsafe.SkipInit(out JpegBlock8x8F tempFBuffer);

        JpegBlock8x8 outputBuffer;

        for (int rowMcu = 0; rowMcu < mcusPerColumn; rowMcu++)
        {
            int offsetY = rowMcu * maxVerticalSampling;
            for (int colMcu = 0; colMcu < mcusPerLine; colMcu++)
            {
                int offsetX = colMcu * maxHorizontalSampling;

                // Scan an interleaved mcu... process components in order
                foreach (JpegArithmeticDecodingComponent component in components)
                {
                    int index = component.ComponentIndex;
                    int h = component.HorizontalSamplingFactor;
                    int v = component.VerticalSamplingFactor;
                    int hs = component.HorizontalSubsamplingFactor;
                    int vs = component.VerticalSubsamplingFactor;

                    for (int y = 0; y < v; y++)
                    {
                        int blockOffsetY = (offsetY + y) * 8;
                        for (int x = 0; x < h; x++)
                        {
                            // Read MCU
                            outputBuffer = default;
                            ReadBlock(ref bitReader, component, ref outputBuffer);

                            // Dequantization
                            DequantizeBlockAndUnZigZag(component.QuantizationTable, ref outputBuffer, ref blockFBuffer);

                            // IDCT
                            FastFloatingPointDCT.TransformIDCT(ref blockFBuffer, ref outputFBuffer, ref tempFBuffer);

                            // Level shift
                            ShiftDataLevel(ref outputFBuffer, ref outputBuffer, levelShift);

                            // CopyToOutput
                            WriteBlock(outputWriter, ref Unsafe.As<JpegBlock8x8, short>(ref outputBuffer), index, (offsetX + x) * 8, blockOffsetY, hs, vs);
                        }
                    }
                }

                // Handle restart
                if (_restartInterval > 0 && (--mcusBeforeRestart) == 0)
                {
                    bitReader.AdvanceAlignByte();

                    JpegMarker marker = bitReader.TryReadMarker();
                    if (marker == JpegMarker.EndOfImage)
                    {
                        int bytesConsumedEoi = reader.RemainingByteCount - bitReader.RemainingBits / 8;
                        reader.TryAdvance(bytesConsumedEoi - 2);
                        return;
                    }
                    if (!marker.IsRestartMarker())
                    {
                        ThrowInvalidDataException("Restart marker is expected.");
                    }

                    mcusBeforeRestart = _restartInterval;

                    foreach (JpegArithmeticDecodingComponent component in components)
                    {
                        component.DcPredictor = 0;
                        component.DcContext = 0;
                        component.DcStatistics?.Reset();
                        component.AcStatistics?.Reset();
                    }

                    Reset();
                }
            }
        }

        bitReader.AdvanceAlignByte();
        int bytesConsumed = reader.RemainingByteCount - bitReader.RemainingBits / 8;
        if (bitReader.TryPeekMarker() != 0)
        {
            if (!bitReader.TryPeekMarker().IsRestartMarker())
            {
                bytesConsumed -= 2;
            }
        }
        reader.TryAdvance(bytesConsumed);
    }

    private void ReadBlock(
        ref JpegBitReader reader, JpegArithmeticDecodingComponent component, scoped ref JpegBlock8x8 destinationBlock)
    {
        ref short destinationRef = ref Unsafe.As<JpegBlock8x8, short>(ref destinationBlock);

        // Sections F.2.4.1 & F.1.4.4.1: Decoding of DC coefficients

        // Table F.4: Point to statistics bin S0 for DC coefficient coding
        ref byte st = ref Unsafe.Add(ref component.DcStatistics!.GetReference(), component.DcContext);

        /* Figure F.19: Decode_DC_DIFF */
        if (DecodeBinaryDecision(ref reader, ref st) == 0)
        {
            component.DcContext = 0;
        }
        else
        {
            // Figure F.21: Decoding nonzero value v
            // Figure F.22: Decoding the sign of v
            int sign = DecodeBinaryDecision(ref reader, ref Unsafe.Add(ref st, 1));
            st = ref Unsafe.Add(ref st, 2 + sign);
            // Figure F.23: Decoding the magnitude category of v
            int m = DecodeBinaryDecision(ref reader, ref st);
            if (m != 0)
            {
                st = ref component.DcStatistics!.GetReference(20);
                while (DecodeBinaryDecision(ref reader, ref st) != 0)
                {
                    if ((m <<= 1) == 0x8000)
                    {
                        ThrowInvalidDataException("Invalid arithmetic code.");
                    }
                    st = ref Unsafe.Add(ref st, 1);
                }
            }
            // Section F.1.4.4.1.2: Establish dc_context conditioning category
            if (m < (int)((1L << component.DcTable!.DcL) >> 1))
            {
                component.DcContext = 0; // zero diff category
            }
            else if (m > (int)((1L << component.DcTable!.DcU) >> 1))
            {
                component.DcContext = 12 + (sign * 4); // large diff category
            }
            else
            {
                component.DcContext = 4 + (sign * 4);  // small diff category
            }
            int v = m;
            // Figure F.24: Decoding the magnitude bit pattern of v
            st = ref Unsafe.Add(ref st, 14);
            while ((m >>= 1) != 0)
            {
                if (DecodeBinaryDecision(ref reader, ref st) != 0)
                {
                    v |= m;
                }
            }
            v += 1;
            if (sign != 0)
            {
                v = -v;
            }
            component.DcPredictor = (short)(component.DcPredictor + v);
        }

        destinationRef = (short)component.DcPredictor;

        // Sections F.2.4.2 & F.1.4.4.2: Decoding of AC coefficients
        JpegArithmeticStatistics acStatistics = component.AcStatistics!;
        JpegArithmeticDecodingTable acTable = component.AcTable!;

        for (int k = 1; k <= 63; k++)
        {
            st = ref acStatistics.GetReference(3 * (k - 1));
            if (DecodeBinaryDecision(ref reader, ref st) != 0)
            {
                // EOB flag
                break;
            }
            while (DecodeBinaryDecision(ref reader, ref Unsafe.Add(ref st, 1)) == 0)
            {
                st = ref Unsafe.Add(ref st, 3);
                k++;
                if (k > 63)
                {
                    ThrowInvalidDataException("Invalid arithmetic code.");
                }
            }
            // Figure F.21: Decoding nonzero value v
            // Figure F.22: Decoding the sign of v
            int sign = DecodeBinaryDecision(ref reader, ref GetFixedBinReference());
            st = ref Unsafe.Add(ref st, 2);
            // Figure F.23: Decoding the magnitude category of v
            int m = DecodeBinaryDecision(ref reader, ref st);
            if (m != 0)
            {
                if (DecodeBinaryDecision(ref reader, ref st) != 0)
                {
                    m <<= 1;
                    st = ref acStatistics.GetReference(k <= acTable.AcKx ? 189 : 217);
                    while (DecodeBinaryDecision(ref reader, ref st) != 0)
                    {
                        if ((m <<= 1) == 0x8000)
                        {
                            ThrowInvalidDataException("Invalid arithmetic code.");
                        }
                        st = ref Unsafe.Add(ref st, 1);
                    }
                }
            }
            int v = m;
            // Figure F.24: Decoding the magnitude bit pattern of v
            st = ref Unsafe.Add(ref st, 14);
            while ((m >>= 1) != 0)
            {
                if (DecodeBinaryDecision(ref reader, ref st) != 0)
                {
                    v |= m;
                }
            }
            v += 1;
            if (sign != 0)
            {
                v = -v;
            }
            Unsafe.Add(ref destinationRef, k) = (short)v;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteBlock(JpegBlockOutputWriter outputWriter, ref short blockRef, int componentIndex, int x, int y, int horizontalSubsamplingFactor, int verticalSubsamplingFactor)
    {
        if (horizontalSubsamplingFactor == 1 && verticalSubsamplingFactor == 1)
        {
            outputWriter!.WriteBlock(ref blockRef, componentIndex, x, y);
        }
        else
        {
            WriteBlockSlow(outputWriter, ref blockRef, componentIndex, x, y, horizontalSubsamplingFactor, verticalSubsamplingFactor);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void WriteBlockSlow(JpegBlockOutputWriter outputWriter, ref short blockRef, int componentIndex, int x, int y, int horizontalSubsamplingFactor, int verticalSubsamplingFactor)
    {
        Unsafe.SkipInit(out JpegBlock8x8 tempBlock);

        int hShift = JpegMathHelper.Log2((uint)horizontalSubsamplingFactor);
        int vShift = JpegMathHelper.Log2((uint)verticalSubsamplingFactor);

        ref short tempRef = ref Unsafe.As<JpegBlock8x8, short>(ref Unsafe.AsRef(in tempBlock));

        for (int v = 0; v < verticalSubsamplingFactor; v++)
        {
            for (int h = 0; h < horizontalSubsamplingFactor; h++)
            {
                int vBlock = 8 * v;
                int hBlock = 8 * h;
                // Fill tempBlock
                for (int i = 0; i < 8; i++)
                {
                    ref short tempRowRef = ref Unsafe.Add(ref tempRef, 8 * i);
                    ref short blockRowRef = ref Unsafe.Add(ref blockRef, ((vBlock + i) >> vShift) * 8);
                    for (int j = 0; j < 8; j++)
                    {
                        Unsafe.Add(ref tempRowRef, j) = Unsafe.Add(ref blockRowRef, (hBlock + j) >> hShift);
                    }
                }

                // Write tempBlock to output
                outputWriter.WriteBlock(ref tempRef, componentIndex, x + 8 * h, y + 8 * v);
            }
        }
    }

    public override void Dispose()
    {
        // Do nothing
    }

}
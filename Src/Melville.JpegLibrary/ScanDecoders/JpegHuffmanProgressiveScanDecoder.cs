using System.Runtime.CompilerServices;
using Melville.JpegLibrary.BlockOutputWriters;
using Melville.JpegLibrary.Decoder;
using Melville.JpegLibrary.HuffmanCoding;
using Melville.JpegLibrary.Maths;
using Melville.JpegLibrary.Readers;

namespace Melville.JpegLibrary.ScanDecoders;

internal sealed class JpegHuffmanProgressiveScanDecoder : JpegHuffmanScanDecoder
{
    private readonly JpegFrameHeader _frameHeader;

    private readonly int _mcusPerLine;
    private readonly int _mcusPerColumn;
    private readonly int _levelShift;
    private ushort _restartInterval;
    private int _mcusBeforeRestart;
    private int _eobrun;

    private readonly JpegBlockOutputWriter _outputWriter;
    private readonly JpegBlockAllocator _allocator;
    private readonly JpegHuffmanDecodingComponent[] _components;

    public JpegHuffmanProgressiveScanDecoder(JpegDecoder decoder, JpegFrameHeader frameHeader) : base(decoder)
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

        _mcusPerLine = (frameHeader.SamplesPerLine + 8 * maxHorizontalSampling - 1) / (8 * maxHorizontalSampling);
        _mcusPerColumn = (frameHeader.NumberOfLines + 8 * maxVerticalSampling - 1) / (8 * maxVerticalSampling);
        _levelShift = 1 << (frameHeader.SamplePrecision - 1);

        JpegBlockOutputWriter? outputWriter = decoder.GetOutputWriter();
        if (outputWriter is null)
        {
            ThrowInvalidDataException("Output writer is not set.");
        }
        _outputWriter = outputWriter;
        _allocator = new JpegBlockAllocator(decoder.MemoryPool);
        _allocator.Allocate(frameHeader);

        // Pre-allocate the JpegDecodeComponent instances
        _components = new JpegHuffmanDecodingComponent[frameHeader.NumberOfComponents];
        for (int i = 0; i < _components.Length; i++)
        {
            _components[i] = new JpegHuffmanDecodingComponent();
        }
    }

    public override void ProcessScan(ref JpegReader reader, JpegScanHeader scanHeader)
    {
        if (scanHeader.Components is null)
        {
            throw new InvalidOperationException();
        }
        if (Decoder.GetOutputWriter() is null)
        {
            throw new InvalidOperationException();
        }

        // Resolve each component
        Span<JpegHuffmanDecodingComponent> components = _components.AsSpan(0, InitDecodeComponents(_frameHeader, scanHeader, _components));
        foreach (JpegHuffmanDecodingComponent component in components)
        {
            if (component.QuantizationTable.IsEmpty)
            {
                ThrowInvalidDataException($"Quantization table of component {component.ComponentIndex} is not defined.");
            }
        }

        _restartInterval = Decoder.GetRestartInterval();
        _mcusBeforeRestart = _restartInterval;
        _eobrun = 0;

        if (components.Length == 1)
        {
            DecodeProgressiveDataNonInterleaved(ref reader, scanHeader, components[0]);
        }
        else
        {
            DecodeProgressiveDataInterleaved(ref reader, scanHeader, components);
        }
    }

    private void DecodeProgressiveDataInterleaved(ref JpegReader reader, JpegScanHeader scanHeader, Span<JpegHuffmanDecodingComponent> components)
    {
        foreach (JpegHuffmanDecodingComponent component in components)
        {
            if (component.DcTable is null)
            {
                ThrowInvalidDataException($"Huffman table of component {component.ComponentIndex} is not defined.");
            }
        }

        JpegBlockAllocator allocator = _allocator;
        JpegBitReader bitReader = new JpegBitReader(reader.RemainingBytes);

        int mcusPerColumn = _mcusPerColumn;
        int mcusPerLine = _mcusPerLine;

        for (int rowMcu = 0; rowMcu < mcusPerColumn; rowMcu++)
        {
            for (int colMcu = 0; colMcu < mcusPerLine; colMcu++)
            {
                foreach (JpegHuffmanDecodingComponent component in components)
                {
                    int index = component.ComponentIndex;
                    int h = component.HorizontalSamplingFactor;
                    int v = component.VerticalSamplingFactor;
                    int offsetX = colMcu * h;
                    int offsetY = rowMcu * v;

                    for (int y = 0; y < v; y++)
                    {
                        int blockOffsetY = offsetY + y;
                        for (int x = 0; x < h; x++)
                        {
                            ref JpegBlock8x8 blockRef = ref allocator.GetBlockReference(index, offsetX + x, blockOffsetY);

                            ReadBlockProgressiveDC(ref bitReader, component, scanHeader, ref blockRef);
                        }
                    }
                }

                if (!HandleRestart(ref bitReader, ref reader))
                {
                    return;
                }
            }
        }
    }

    private void DecodeProgressiveDataNonInterleaved(ref JpegReader reader, JpegScanHeader scanHeader, JpegHuffmanDecodingComponent component)
    {
        JpegBlockAllocator allocator = _allocator;
        JpegBitReader bitReader = new JpegBitReader(reader.RemainingBytes);

        int componentIndex = component.ComponentIndex;
        int horizontalBlockCount = (_frameHeader.SamplesPerLine + 8 * component.HorizontalSubsamplingFactor - 1) / (8 * component.HorizontalSubsamplingFactor);
        int verticalBlockCount = (_frameHeader.NumberOfLines + 8 * component.VerticalSubsamplingFactor - 1) / (8 * component.VerticalSubsamplingFactor);

        if (scanHeader.StartOfSpectralSelection == 0)
        {
            if (component.DcTable is null)
            {
                ThrowInvalidDataException($"Huffman table of component {componentIndex} is not defined.");
            }

            for (int blockY = 0; blockY < verticalBlockCount; blockY++)
            {
                for (int blockX = 0; blockX < horizontalBlockCount; blockX++)
                {
                    ref JpegBlock8x8 blockRef = ref allocator.GetBlockReference(componentIndex, blockX, blockY);

                    ReadBlockProgressiveDC(ref bitReader, component, scanHeader, ref blockRef);

                    if (!HandleRestart(ref bitReader, ref reader))
                    {
                        return;
                    }
                }
            }
        }
        else
        {
            JpegHuffmanDecodingTable? acTable = component.AcTable;
            if (acTable is null)
            {
                ThrowInvalidDataException($"Huffman table of component {componentIndex} is not defined.");
            }

            for (int blockY = 0; blockY < verticalBlockCount; blockY++)
            {
                for (int blockX = 0; blockX < horizontalBlockCount; blockX++)
                {
                    ref JpegBlock8x8 blockRef = ref allocator.GetBlockReference(componentIndex, blockX, blockY);

                    ReadBlockProgressiveAC(ref bitReader, acTable, scanHeader, ref _eobrun, ref blockRef);

                    if (!HandleRestart(ref bitReader, ref reader))
                    {
                        return;
                    }
                }
            }
        }
    }

    private bool HandleRestart(ref JpegBitReader bitReader, ref JpegReader reader)
    {
        if (_restartInterval > 0 && (--_mcusBeforeRestart) == 0)
        {
            bitReader.AdvanceAlignByte();

            JpegMarker marker = bitReader.TryReadMarker();
            if (marker == JpegMarker.EndOfImage)
            {
                int bytesConsumedEoi = reader.RemainingByteCount - bitReader.RemainingBits / 8;
                reader.TryAdvance(bytesConsumedEoi - 2);
                return false;
            }
            if (!marker.IsRestartMarker())
            {
                throw new InvalidOperationException("Expect restart marker.");
            }

            _mcusBeforeRestart = _restartInterval;
            _eobrun = 0;

            foreach (JpegHuffmanDecodingComponent component in _components)
            {
                component.DcPredictor = 0;
            }
        }

        return true;
    }


    private static void ReadBlockProgressiveDC(ref JpegBitReader reader, JpegHuffmanDecodingComponent component, JpegScanHeader scanHeader, ref JpegBlock8x8 destinationBlock)
    {
        ref short blockDataRef = ref Unsafe.As<JpegBlock8x8, short>(ref destinationBlock);

        if (scanHeader.SuccessiveApproximationBitPositionHigh == 0)
        {
            // First scan
            int s = DecodeHuffmanCode(ref reader, component.DcTable!);
            if (s != 0)
            {
                s = ReceiveAndExtend(ref reader, s);
            }

            s += component.DcPredictor;
            component.DcPredictor = s;
            blockDataRef = (short)(s << scanHeader.SuccessiveApproximationBitPositionLow);
        }
        else
        {
            // Refinement scan
            if (!reader.TryReadBits(1, out int bits, out _))
            {
                ThrowInvalidDataException("Unexpected end of JPEG data stream.");
            }
            blockDataRef |= (short)(bits << scanHeader.SuccessiveApproximationBitPositionLow);
        }
    }

    private static void ReadBlockProgressiveAC(ref JpegBitReader reader, JpegHuffmanDecodingTable acTable, JpegScanHeader scanHeader, ref int eobrun, ref JpegBlock8x8 destinationBlock)
    {
        ref short blockDataRef = ref Unsafe.As<JpegBlock8x8, short>(ref destinationBlock);

        if (scanHeader.SuccessiveApproximationBitPositionHigh == 0)
        {
            // AC initial scan
            if (eobrun != 0)
            {
                eobrun--;
                return;
            }

            int start = scanHeader.StartOfSpectralSelection;
            int end = scanHeader.EndOfSpectralSelection;
            int low = scanHeader.SuccessiveApproximationBitPositionLow;

            for (int i = start; i <= end; i++)
            {
                int s = DecodeHuffmanCode(ref reader, acTable);

                int r = s >> 4;
                s &= 15;

                i += r;

                if (s != 0)
                {
                    s = ReceiveAndExtend(ref reader, s);
                    Unsafe.Add(ref blockDataRef, Math.Min(i, 63)) = (short)(s << low);
                }
                else
                {
                    if (r != 15)
                    {
                        eobrun = 1 << r;
                        if (r != 0)
                        {
                            if (!reader.TryReadBits(r, out int bits, out _))
                            {
                                ThrowInvalidDataException("Unexpected end of JPEG data stream.");
                            }
                            eobrun += bits;
                        }

                        --eobrun;
                        break;
                    }
                }
            }
        }
        else
        {
            // Refinement scan
            ReadBlockProgressiveACRefined(ref reader, acTable, scanHeader, ref eobrun, ref blockDataRef);
        }
    }

    private static void ReadBlockProgressiveACRefined(ref JpegBitReader reader, JpegHuffmanDecodingTable acTable, JpegScanHeader scanHeader, ref int eobrun, ref short blockDataRef)
    {
        int start = scanHeader.StartOfSpectralSelection;
        int end = scanHeader.EndOfSpectralSelection;

        int p1 = 1 << scanHeader.SuccessiveApproximationBitPositionLow;
        int m1 = (-1) << scanHeader.SuccessiveApproximationBitPositionLow;

        int k = start;

        if (eobrun == 0)
        {
            for (; k <= end; k++)
            {
                int s = DecodeHuffmanCode(ref reader, acTable);

                int r = s >> 4;
                s &= 15;

                if (s != 0)
                {
                    if (!reader.TryReadBits(1, out int bits, out _))
                    {
                        ThrowInvalidDataException("Unexpected end of JPEG data stream.");
                    }
                    s = bits != 0 ? p1 : m1;
                }
                else
                {
                    if (r != 15)
                    {
                        eobrun = 1 << r;

                        if (r != 0)
                        {
                            if (!reader.TryReadBits(r, out int bits, out _))
                            {
                                ThrowInvalidDataException("Unexpected end of JPEG data stream.");
                            }
                            eobrun += bits;
                        }

                        break;
                    }
                }

                do
                {
                    ref short coef = ref Unsafe.Add(ref blockDataRef, k);
                    if (coef != 0)
                    {
                        if (!reader.TryReadBits(1, out int bits, out _))
                        {
                            ThrowInvalidDataException("Unexpected end of JPEG data stream.");
                        }
                        if (bits != 0)
                        {
                            if ((coef & p1) == 0)
                            {
                                coef += (short)(coef >= 0 ? p1 : m1);
                            }
                        }
                    }
                    else
                    {
                        if (--r < 0)
                        {
                            break;
                        }
                    }

                    k++;
                } while (k <= end);

                if ((s != 0) && (k < 64))
                {
                    Unsafe.Add(ref blockDataRef, k) = (short)s;
                }
            }
        }

        if (eobrun > 0)
        {
            for (; k <= end; k++)
            {
                ref short coef = ref Unsafe.Add(ref blockDataRef, k);

                if (coef != 0)
                {
                    if (!reader.TryReadBits(1, out int bits, out _))
                    {
                        ThrowInvalidDataException("Unexpected end of JPEG data stream.");
                    }
                    if (bits != 0)
                    {
                        if ((coef & p1) == 0)
                        {
                            coef += (short)(coef > 0 ? p1 : m1);
                        }
                    }

                }
            }

            --eobrun;
        }
    }

    public override void Dispose()
    {
        JpegBlockAllocator allocator = _allocator;

        int mcusPerColumn = _mcusPerColumn;
        int mcusPerLine = _mcusPerLine;
        int levelShift = _levelShift;
        JpegHuffmanDecodingComponent[] components = _components;

        Unsafe.SkipInit(out JpegBlock8x8F blockFBuffer);
        Unsafe.SkipInit(out JpegBlock8x8F outputFBuffer);
        Unsafe.SkipInit(out JpegBlock8x8F tempFBuffer);

        for (int rowMcu = 0; rowMcu < mcusPerColumn; rowMcu++)
        {
            for (int colMcu = 0; colMcu < mcusPerLine; colMcu++)
            {
                // Scan an interleaved mcu... process components in order
                foreach (JpegHuffmanDecodingComponent component in components)
                {
                    int index = component.ComponentIndex;
                    int h = component.HorizontalSamplingFactor;
                    int v = component.VerticalSamplingFactor;
                    int offsetX = colMcu * h;
                    int offsetY = rowMcu * v;

                    for (int y = 0; y < v; y++)
                    {
                        int blockOffsetY = offsetY + y;
                        for (int x = 0; x < h; x++)
                        {
                            ref JpegBlock8x8 blockRef = ref allocator.GetBlockReference(index, offsetX + x, blockOffsetY);

                            // Dequantization
                            DequantizeBlockAndUnZigZag(component.QuantizationTable, ref blockRef, ref blockFBuffer);

                            // IDCT
                            FastFloatingPointDCT.TransformIDCT(ref blockFBuffer, ref outputFBuffer, ref tempFBuffer);

                            // Level shift
                            ShiftDataLevel(ref outputFBuffer, ref blockRef, levelShift);
                        }
                    }
                }
            }
        }

        allocator.Flush(_outputWriter);
        allocator.Dispose();
    }

}
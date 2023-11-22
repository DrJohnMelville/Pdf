using System.Diagnostics;
using System.Runtime.CompilerServices;
using Melville.JpegLibrary.BlockOutputWriters;
using Melville.JpegLibrary.Decoder;
using Melville.JpegLibrary.HuffmanCoding;
using Melville.JpegLibrary.Maths;
using Melville.JpegLibrary.Readers;

namespace Melville.JpegLibrary.ScanDecoders;

    internal abstract class JpegHuffmanScanDecoder : JpegScanDecoder
    {
        protected JpegDecoder Decoder { get; private set; }

        public JpegHuffmanScanDecoder(JpegDecoder decoder)
        {
            Decoder = decoder;
        }

        protected int InitDecodeComponents(JpegFrameHeader frameHeader, JpegScanHeader scanHeader, Span<JpegHuffmanDecodingComponent> components)
        {
            Debug.Assert(frameHeader.Components is not null);
            Debug.Assert(scanHeader.Components is not null);

            // Compute maximum sampling factor
            int maxHorizontalSampling = 1;
            int maxVerticalSampling = 1;
            foreach (JpegFrameComponentSpecificationParameters currentFrameComponent in frameHeader.Components!)
            {
                maxHorizontalSampling = Math.Max(maxHorizontalSampling, currentFrameComponent.HorizontalSamplingFactor);
                maxVerticalSampling = Math.Max(maxVerticalSampling, currentFrameComponent.VerticalSamplingFactor);
            }

            // Resolve each component
            if (components.Length < scanHeader.NumberOfComponents)
            {
                throw new InvalidOperationException();
            }
            for (int i = 0; i < scanHeader.NumberOfComponents; i++)
            {
                JpegScanComponentSpecificationParameters scanComponenet = scanHeader.Components![i];
                int componentIndex = 0;
                JpegFrameComponentSpecificationParameters? frameComponent = null;

                for (int j = 0; j < frameHeader.NumberOfComponents; j++)
                {
                    JpegFrameComponentSpecificationParameters currentFrameComponent = frameHeader.Components[j];
                    if (scanComponenet.ScanComponentSelector == currentFrameComponent.Identifier)
                    {
                        componentIndex = j;
                        frameComponent = currentFrameComponent;
                    }
                }
                if (frameComponent is null)
                {
                    ThrowInvalidDataException("The specified component is missing.");
                }
                JpegHuffmanDecodingComponent component = components[i];
                if (component is null)
                {
                    components[i] = component = new JpegHuffmanDecodingComponent();
                }
                component.ComponentIndex = componentIndex;
                component.HorizontalSamplingFactor = frameComponent.GetValueOrDefault().HorizontalSamplingFactor;
                component.VerticalSamplingFactor = frameComponent.GetValueOrDefault().VerticalSamplingFactor;
                component.DcTable = Decoder.GetHuffmanTable(true, scanComponenet.DcEntropyCodingTableSelector);
                component.AcTable = Decoder.GetHuffmanTable(false, scanComponenet.AcEntropyCodingTableSelector);
                component.QuantizationTable = Decoder.GetQuantizationTable(frameComponent.GetValueOrDefault().QuantizationTableSelector);
                component.HorizontalSubsamplingFactor = maxHorizontalSampling / component.HorizontalSamplingFactor;
                component.VerticalSubsamplingFactor = maxVerticalSampling / component.VerticalSamplingFactor;
                component.DcPredictor = 0;
            }

            return scanHeader.NumberOfComponents;
        }

        protected JpegHuffmanDecodingComponent[] InitDecodeComponents(JpegFrameHeader frameHeader, JpegScanHeader scanHeader)
        {
            JpegHuffmanDecodingComponent[] components = new JpegHuffmanDecodingComponent[scanHeader.NumberOfComponents];
            InitDecodeComponents(frameHeader, scanHeader, components);
            return components;
        }

        protected static int DecodeHuffmanCode(ref JpegBitReader reader, JpegHuffmanDecodingTable table)
        {
            int bits = reader.PeekBits(16, out int bitsRead);
            JpegHuffmanDecodingTable.Entry entry = table.Lookup(bits);
            bitsRead = Math.Min(entry.CodeSize, bitsRead);
            _ = reader.TryAdvanceBits(bitsRead, out _);
            return entry.SymbolValue;
        }

        protected static JpegHuffmanDecodingTable.Entry DecodeHuffmanCode(ref JpegBitReader reader, JpegHuffmanDecodingTable table, out int code, out int bitsRead)
        {
            int bits = reader.PeekBits(16, out bitsRead);
            JpegHuffmanDecodingTable.Entry entry = table.Lookup(bits);
            bitsRead = Math.Min(entry.CodeSize, bitsRead);
            _ = reader.TryAdvanceBits(bitsRead, out _);
            code = bits >> (16 - bitsRead);
            return entry;
        }

        protected static int ReceiveAndExtend(ref JpegBitReader reader, int length)
        {
            Debug.Assert(length > 0);
            if (!reader.TryReadBits(length, out int value, out bool isMarkerEncountered))
            {
                if (isMarkerEncountered)
                {
                    ThrowInvalidDataException("Expect raw data from bit stream. Yet a marker is encountered.");
                }
                ThrowInvalidDataException("The bit stream ended prematurely.");
            }

            return Extend(value, length);

            static int Extend(int v, int nbits) => v - ((((v + v) >> nbits) - 1) & ((1 << nbits) - 1));
        }
    }
internal sealed class JpegHuffmanBaselineScanDecoder : JpegHuffmanScanDecoder
{
    private readonly JpegFrameHeader _frameHeader;

    private readonly int _maxHorizontalSampling;
    private readonly int _maxVerticalSampling;

    private readonly ushort _restartInterval;
    private readonly int _mcusPerLine;
    private readonly int _mcusPerColumn;
    private readonly int _levelShift;

    private readonly JpegHuffmanDecodingComponent[] _components;

    public JpegHuffmanBaselineScanDecoder(JpegDecoder decoder, JpegFrameHeader frameHeader) : base(decoder)
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
        _components = new JpegHuffmanDecodingComponent[frameHeader.NumberOfComponents];
        for (int i = 0; i < _components.Length; i++)
        {
            _components[i] = new JpegHuffmanDecodingComponent();
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
        Span<JpegHuffmanDecodingComponent> components = _components.AsSpan(0, InitDecodeComponents(frameHeader, scanHeader, _components));
        foreach (JpegHuffmanDecodingComponent component in components)
        {
            if (component.DcTable is null || component.AcTable is null)
            {
                ThrowInvalidDataException($"Huffman table of component {component.ComponentIndex} is not defined.");
            }
            if (component.QuantizationTable.IsEmpty)
            {
                ThrowInvalidDataException($"Quantization table of component {component.ComponentIndex} is not defined.");
            }
        }

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
                foreach (JpegHuffmanDecodingComponent component in components)
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
                            ReadBlockBaseline(ref bitReader, component, ref outputBuffer);

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
                        throw new InvalidOperationException("Expect restart marker.");
                    }

                    mcusBeforeRestart = _restartInterval;

                    foreach (JpegHuffmanDecodingComponent component in components)
                    {
                        component.DcPredictor = 0;
                    }

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

    private static void ReadBlockBaseline(
        ref JpegBitReader reader, JpegHuffmanDecodingComponent component, scoped ref JpegBlock8x8 destinationBlock)
    {
        ref short destinationRef = ref Unsafe.As<JpegBlock8x8, short>(ref destinationBlock);

        Debug.Assert(component.DcTable is not null);
        Debug.Assert(component.AcTable is not null);

        // DC
        int t = DecodeHuffmanCode(ref reader, component.DcTable!);
        if (t != 0)
        {
            t = ReceiveAndExtend(ref reader, t);
        }

        t += component.DcPredictor;
        component.DcPredictor = t;
        destinationRef = (short)t;

        // AC
        JpegHuffmanDecodingTable acTable = component.AcTable!;
        for (int i = 1; i < 64;)
        {
            int s = DecodeHuffmanCode(ref reader, acTable);

            int r = s >> 4;
            s &= 15;

            if (s != 0)
            {
                i += r;
                s = ReceiveAndExtend(ref reader, s);
                Unsafe.Add(ref destinationRef, Math.Min(i++, 63)) = (short)s;
            }
            else
            {
                if (r == 0)
                {
                    break;
                }

                i += 16;
            }
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
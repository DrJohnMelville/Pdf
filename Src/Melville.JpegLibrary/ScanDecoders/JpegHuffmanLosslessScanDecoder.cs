using System.Runtime.CompilerServices;
using Melville.JpegLibrary.BlockOutputWriters;
using Melville.JpegLibrary.Decoder;
using Melville.JpegLibrary.HuffmanCoding;
using Melville.JpegLibrary.Readers;

namespace Melville.JpegLibrary.ScanDecoders;

internal sealed class JpegHuffmanLosslessScanDecoder : JpegHuffmanScanDecoder
{
    private readonly JpegFrameHeader _frameHeader;

    private readonly ushort _restartInterval;
    private readonly int _mcusPerLine;
    private readonly int _mcusPerColumn;

    private readonly JpegPartialScanlineAllocator _allocator;
    private readonly JpegHuffmanDecodingComponent[] _components;

    public JpegHuffmanLosslessScanDecoder(JpegDecoder decoder, JpegFrameHeader frameHeader) : base(decoder)
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

        _restartInterval = decoder.GetRestartInterval();
        _mcusPerLine = (frameHeader.SamplesPerLine + maxHorizontalSampling - 1) / maxHorizontalSampling;
        _mcusPerColumn = (frameHeader.NumberOfLines + maxVerticalSampling - 1) / maxVerticalSampling;

        JpegBlockOutputWriter? outputWriter = decoder.GetOutputWriter();
        if (outputWriter is null)
        {
            ThrowInvalidDataException("Output writer is not set.");
        }
        _allocator = new JpegPartialScanlineAllocator(outputWriter, decoder.MemoryPool);
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
            if (component.DcTable is null)
            {
                ThrowInvalidDataException($"Huffman table of component {component.ComponentIndex} is not defined.");
            }
        }

        JpegPartialScanlineAllocator allocator = _allocator;
        int mcusPerLine = _mcusPerLine;
        int mcusPerColumn = _mcusPerColumn;

        // Prepare
        JpegBitReader bitReader = new JpegBitReader(reader.RemainingBytes);
        int restartInterval = _restartInterval;
        int mcusBeforeRestart = restartInterval;
        int predictor = scanHeader.StartOfSpectralSelection;
        int initialPrediction = 1 << (_frameHeader.SamplePrecision - scanHeader.SuccessiveApproximationBitPositionLow - 1);

        for (int rowMcu = 0; rowMcu < mcusPerColumn; rowMcu++)
        {
            for (int colMcu = 0; colMcu < mcusPerLine; colMcu++)
            {
                // Scan an interleaved mcu... process components in order
                foreach (JpegHuffmanDecodingComponent component in components)
                {
                    int index = component.ComponentIndex;
                    JpegHuffmanDecodingTable losslessTable = component.DcTable!;
                    int h = component.HorizontalSamplingFactor;
                    int v = component.VerticalSamplingFactor;
                    int offsetX = colMcu * h;
                    int offsetY = rowMcu * v;

                    for (int y = 0; y < v; y++)
                    {
                        Span<short> scanline = allocator.GetScanlineSpan(index, offsetY + y);
                        Span<short> lastScanline = (y == 0 && rowMcu == 0) ? default : allocator.GetScanlineSpan(index, offsetY + y - 1);

                        for (int x = 0; x < h; x++)
                        {
                            int diffValue = ReadSampleLossless(ref bitReader, losslessTable);

                            // The one-dimensional horizontal predictor (prediction sample Ra) is used 
                            // for the first line of samples at the start of the scan and at the beginning of each restart interval
                            if (rowMcu == 0 || (restartInterval > 0 && mcusBeforeRestart == restartInterval))
                            {
                                // At the beginning of the first line and at the beginning of each restart interval the prediction value of 2^(P – 1) is used, where P is the input precision.
                                // If the point transformation parameter (see A.4) is non-zero, the prediction value at the beginning of the first lines and the beginning of each restart interval is 2^(P – Pt – 1), where Pt is the value of the point transformation parameter.
                                if (colMcu == 0 && x == 0)
                                {
                                    diffValue += initialPrediction;
                                }
                                else
                                {
                                    int ra = scanline[offsetX + x - 1];
                                    int rb = y == 0 ? initialPrediction : lastScanline[offsetX + x];
                                    int rc = y == 0 ? initialPrediction : lastScanline[offsetX + x - 1];
                                    diffValue += predictor switch
                                    {
                                        1 => ra,// Px = Ra
                                        2 => rb,// Px = Rb
                                        3 => rc,// Px = Rc
                                        4 => ra + rb - rc,// Px = Ra + Rb – Rc
                                        5 => ra + ((rb - rc) >> 1),// Px = Ra + (Rb – Rc)/2
                                        6 => rb + ((ra - rc) >> 1),// Px = Rb + (Ra – Rc)/2
                                        7 => (ra + rb) >> 1,// Px = (Ra + Rb)/2
                                        _ => 0,// No prediction (See Annex J)
                                    };
                                }
                            }
                            // The sample from the line above(prediction sample Rb) is used at the start of each line, except for the first line
                            else if (colMcu == 0)
                            {
                                diffValue += lastScanline[offsetX + x];
                            }
                            else
                            {
                                diffValue += predictor switch
                                {
                                    1 => scanline[offsetX + x - 1],// Px = Ra
                                    2 => lastScanline[offsetX + x],// Px = Rb
                                    3 => lastScanline[offsetX + x - 1],// Px = Rc
                                    4 => scanline[offsetX + x - 1] + lastScanline[offsetX + x] - lastScanline[offsetX + x - 1],// Px = Ra + Rb – Rc
                                    5 => scanline[offsetX + x - 1] + ((lastScanline[offsetX + x] - lastScanline[offsetX + x - 1]) >> 1),// Px = Ra + (Rb – Rc)/2
                                    6 => lastScanline[offsetX + x] + ((scanline[offsetX + x - 1] - lastScanline[offsetX + x - 1]) >> 1),// Px = Rb + (Ra – Rc)/2
                                    7 => (scanline[offsetX + x - 1] + lastScanline[offsetX + x]) >> 1,// Px = (Ra + Rb)/2
                                    _ => 0,// No prediction (See Annex J)
                                };
                            }
                            scanline[offsetX + x] = (short)diffValue;
                        }
                    }
                }

                // Handle restart
                if (restartInterval > 0 && (--mcusBeforeRestart) == 0)
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

                    mcusBeforeRestart = restartInterval;
                }
            }

            // Flush allocator
            if (rowMcu == mcusPerColumn - 1)
            {
                foreach (JpegHuffmanDecodingComponent component in components)
                {
                    allocator.FlushLastMcu(component.ComponentIndex, (rowMcu + 1) * component.VerticalSamplingFactor);
                }
            }
            else
            {
                foreach (JpegHuffmanDecodingComponent component in components)
                {
                    allocator.FlushMcu(component.ComponentIndex, (rowMcu + 1) * component.VerticalSamplingFactor);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadSampleLossless(ref JpegBitReader reader, JpegHuffmanDecodingTable losslessTable)
    {
        int t = DecodeHuffmanCode(ref reader, losslessTable);
        if (t == 16)
        {
            t = 32768;
        }
        else if (t != 0)
        {
            t = ReceiveAndExtend(ref reader, t);
        }

        return t;
    }

    public override void Dispose()
    {
        JpegPartialScanlineAllocator allocator = _allocator;

        allocator.Dispose();
    }

}
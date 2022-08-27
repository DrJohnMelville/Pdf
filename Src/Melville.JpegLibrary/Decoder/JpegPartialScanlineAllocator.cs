using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Melville.JpegLibrary.BlockOutputWriters;
using Melville.JpegLibrary.Maths;

namespace Melville.JpegLibrary.Decoder;

internal sealed class JpegPartialScanlineAllocator
{
    private readonly JpegBlockOutputWriter _writer;
    private readonly MemoryPool<byte> _memoryPool;
    private IMemoryOwner<byte>? _bufferHandle;
    private ComponentAllocation[]? _components;

    internal JpegPartialScanlineAllocator(JpegBlockOutputWriter writer, MemoryPool<byte>? memoryPool = null)
    {
        _writer = writer;
        _memoryPool = memoryPool ?? MemoryPool<byte>.Shared;
        _bufferHandle = null;
        _components = null;
    }

    public void Allocate(JpegFrameHeader frameHeader)
    {
        // Compute maximum sampling factor
        int maxHorizontalSampling = 1;
        int maxVerticalSampling = 1;
        foreach (JpegFrameComponentSpecificationParameters currentFrameComponent in frameHeader.Components!)
        {
            maxHorizontalSampling = Math.Max(maxHorizontalSampling, currentFrameComponent.HorizontalSamplingFactor);
            maxVerticalSampling = Math.Max(maxVerticalSampling, currentFrameComponent.VerticalSamplingFactor);
        }

        ComponentAllocation[] componentAllocations = _components = new ComponentAllocation[frameHeader.NumberOfComponents];
        int index = 0;
        foreach (JpegFrameComponentSpecificationParameters component in frameHeader.Components)
        {
            int horizontalSubsamplingFactor = maxHorizontalSampling / component.HorizontalSamplingFactor;
            int verticalSubsamplingFactor = maxVerticalSampling / component.VerticalSamplingFactor;

            int width = (frameHeader.SamplesPerLine + horizontalSubsamplingFactor - 1) / horizontalSubsamplingFactor;
            int height = (frameHeader.NumberOfLines + verticalSubsamplingFactor - 1) / verticalSubsamplingFactor;

            componentAllocations[index++] = new ComponentAllocation
            {
                Width = width,
                Height = height,
                HorizontalSubsamplingFactor = horizontalSubsamplingFactor,
                VerticalSubsamplingFactor = verticalSubsamplingFactor,
            };
        }

        index = 0;
        for (int i = 0; i < componentAllocations.Length; i++)
        {
            componentAllocations[i].ComponentSampleOffset = index;
            index += componentAllocations[i].Width * 16;
        }

        int length = index * Unsafe.SizeOf<short>();
        IMemoryOwner<byte> bufferHandle = _bufferHandle = _memoryPool.Rent(length);
        bufferHandle.Memory.Span.Slice(0, length).Clear();
    }

    public Span<short> GetScanlineSpan(int componentIndex, int y)
    {
        ComponentAllocation[]? components = _components;
        if (components is null)
        {
            throw new InvalidOperationException();
        }
        Debug.Assert(_bufferHandle != null);
        if ((uint)componentIndex >= (uint)components.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(componentIndex));
        }
        ComponentAllocation component = components[componentIndex];

        if ((uint)y >= (uint)component.Height)
        {
            throw new ArgumentOutOfRangeException(nameof(y));
        }

        Span<short> bufferSpan = MemoryMarshal.Cast<byte, short>(_bufferHandle!.Memory.Span);
        return bufferSpan.Slice(component.ComponentSampleOffset).Slice(component.Width * (y & 0b1111), component.Width);
    }

    public void FlushMcu(int componentIndex, int y)
    {
        // Only flush on the last scanline of 8x8 block.
        if ((y % 8) != 0 || y == 0)
        {
            return;
        }
        y -= 8;

        FlushCore(componentIndex, y, 8);
    }

    public void FlushLastMcu(int componentIndex, int y)
    {
        // Only flush on the last scanline of 8x8 block.
        if (y == 0)
        {
            return;
        }
        int offsetY = (y - 1) / 8 * 8;

        FlushCore(componentIndex, offsetY, y - offsetY);
    }

    private void FlushCore(int componentIndex, int y, int writeHeight)
    {
        ComponentAllocation[]? components = _components;
        if (components is null)
        {
            return;
        }

        Unsafe.SkipInit(out JpegBlock8x8 block);
        JpegBlockOutputWriter? outputWriter = _writer;
        Debug.Assert(outputWriter is not null);

        ComponentAllocation component = components[componentIndex];
        int width = component.Width;

        ref short componentSampleRef = ref Unsafe.As<byte, short>(ref MemoryMarshal.GetReference(_bufferHandle!.Memory.Span));
        componentSampleRef = ref Unsafe.Add(ref componentSampleRef, component.ComponentSampleOffset);
        componentSampleRef = ref Unsafe.Add(ref componentSampleRef, component.Width * (y & 0b1111));

        for (int x = 0; x < width; x += 8)
        {
            int writeWidth = Math.Min(width - x, 8);

            ref short componentRowColRef = ref Unsafe.Add(ref componentSampleRef, x);
            ref short blockRef = ref Unsafe.As<JpegBlock8x8, short>(ref block);

            if (writeHeight == 8 && writeWidth == 8)
            {
                Unsafe.As<short, long>(ref blockRef) = Unsafe.As<short, long>(ref componentRowColRef);
                Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 1) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
                componentRowColRef = ref Unsafe.Add(ref componentRowColRef, width);
                Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 2) = Unsafe.As<short, long>(ref componentRowColRef);
                Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 3) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
                componentRowColRef = ref Unsafe.Add(ref componentRowColRef, width);
                Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 4) = Unsafe.As<short, long>(ref componentRowColRef);
                Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 5) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
                componentRowColRef = ref Unsafe.Add(ref componentRowColRef, width);
                Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 6) = Unsafe.As<short, long>(ref componentRowColRef);
                Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 7) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
                componentRowColRef = ref Unsafe.Add(ref componentRowColRef, width);
                Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 8) = Unsafe.As<short, long>(ref componentRowColRef);
                Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 9) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
                componentRowColRef = ref Unsafe.Add(ref componentRowColRef, width);
                Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 10) = Unsafe.As<short, long>(ref componentRowColRef);
                Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 11) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
                componentRowColRef = ref Unsafe.Add(ref componentRowColRef, width);
                Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 12) = Unsafe.As<short, long>(ref componentRowColRef);
                Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 13) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
                componentRowColRef = ref Unsafe.Add(ref componentRowColRef, width);
                Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 14) = Unsafe.As<short, long>(ref componentRowColRef);
                Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 15) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
            }
            else
            {
                for (int i = 0; i < writeHeight; i++)
                {
                    for (int j = 0; j < writeWidth; j++)
                    {
                        Unsafe.Add(ref blockRef, j) = Unsafe.Add(ref componentRowColRef, i * width + j);
                    }
                    blockRef = ref Unsafe.Add(ref blockRef, 8);
                }
            }

            WriteBlock(outputWriter!, block, componentIndex, component.HorizontalSubsamplingFactor * x, component.VerticalSubsamplingFactor * y, component.HorizontalSubsamplingFactor, component.VerticalSubsamplingFactor);
        }
    }

    private static void WriteBlock(JpegBlockOutputWriter outputWriter, in JpegBlock8x8 block, int componentIndex, int x, int y, int horizontalSamplingFactor, int verticalSamplingFactor)
    {
        ref short blockRef = ref Unsafe.As<JpegBlock8x8, short>(ref Unsafe.AsRef(block));

        if (horizontalSamplingFactor == 1 && verticalSamplingFactor == 1)
        {
            outputWriter!.WriteBlock(ref blockRef, componentIndex, x, y);
        }
        else
        {
            Unsafe.SkipInit(out JpegBlock8x8 tempBlock);

            int hShift = JpegMathHelper.Log2((uint)horizontalSamplingFactor);
            int vShift = JpegMathHelper.Log2((uint)verticalSamplingFactor);

            ref short tempRef = ref Unsafe.As<JpegBlock8x8, short>(ref Unsafe.AsRef(tempBlock));

            for (int v = 0; v < verticalSamplingFactor; v++)
            {
                for (int h = 0; h < horizontalSamplingFactor; h++)
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
                    outputWriter!.WriteBlock(ref tempRef, componentIndex, x + 8 * h, y + 8 * v);
                }
            }
        }
    }

    public void Dispose()
    {
        if (_bufferHandle is not null)
        {
            _bufferHandle.Dispose();
            _bufferHandle = null;
        }
    }

    struct ComponentAllocation
    {
        public int HorizontalSubsamplingFactor;
        public int VerticalSubsamplingFactor;
        public int Width;
        public int Height;
        public int ComponentSampleOffset;
    }
}
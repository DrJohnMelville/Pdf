using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Melville.JpegLibrary.BlockOutputWriters;
using Melville.JpegLibrary.Maths;

namespace Melville.JpegLibrary.Decoder;

public sealed class JpegBlockAllocator : IDisposable
{
    private readonly MemoryPool<byte> _memoryPool;
    private IMemoryOwner<byte>? _bufferHandle;
    private ComponentAllocation[]? _components;

    /// <summary>
    /// Initialize the allocator/>.
    /// </summary>
    /// <param name="memoryPool">The memory pool to use.</param>
    public JpegBlockAllocator(MemoryPool<byte>? memoryPool = null)
    {
        _memoryPool = memoryPool ?? MemoryPool<byte>.Shared;
        _bufferHandle = null;
        _components = null;
    }

    /// <summary>
    /// Allocate blocks for the specified frame.
    /// </summary>
    /// <param name="frameHeader">The information of the frame.</param>
    public void Allocate(JpegFrameHeader frameHeader)
    {
        if (_bufferHandle is not null)
        {
            throw new InvalidOperationException();
        }

        // Compute maximum sampling factor
        int maxHorizontalSampling = 1;
        int maxVerticalSampling = 1;
        foreach (JpegFrameComponentSpecificationParameters currentFrameComponent in frameHeader.Components!)
        {
            maxHorizontalSampling = Math.Max(maxHorizontalSampling, currentFrameComponent.HorizontalSamplingFactor);
            maxVerticalSampling = Math.Max(maxVerticalSampling, currentFrameComponent.VerticalSamplingFactor);
        }

        int horizontalBlockCount = (frameHeader.SamplesPerLine + 7) / 8;
        int verticalBlockCount = (frameHeader.NumberOfLines + 7) / 8;

        ComponentAllocation[] componentAllocations = _components = new ComponentAllocation[frameHeader.NumberOfComponents];
        int index = 0;
        foreach (JpegFrameComponentSpecificationParameters component in frameHeader.Components)
        {
            int horizontalSubsamplingFactor = maxHorizontalSampling / component.HorizontalSamplingFactor;
            int verticalSubsamplingFactor = maxVerticalSampling / component.VerticalSamplingFactor;

            int horizontalComponentBlockCount = (horizontalBlockCount + horizontalSubsamplingFactor - 1) / horizontalSubsamplingFactor;
            int verticalComponentBlockCount = (verticalBlockCount + verticalSubsamplingFactor - 1) / verticalSubsamplingFactor;

            componentAllocations[index++] = new ComponentAllocation
            {
                HorizontalComponentBlock = horizontalComponentBlockCount,
                VerticalComponentBlock = verticalComponentBlockCount,
                HorizontalSubsamplingFactor = horizontalSubsamplingFactor,
                VerticalSubsamplingFactor = verticalSubsamplingFactor,
            };
        }

        // allocate an additional block to act as a dummy buffer.
        index = 1;
        for (int i = 0; i < componentAllocations.Length; i++)
        {
            componentAllocations[i].ComponentBlockOffset = index;
            index += componentAllocations[i].HorizontalComponentBlock * componentAllocations[i].VerticalComponentBlock;
        }

        int length = index * Unsafe.SizeOf<JpegBlock8x8>();
        IMemoryOwner<byte> bufferHandle = _bufferHandle = _memoryPool.Rent(length);
        bufferHandle.Memory.Span.Slice(0, length).Clear();
    }

    /// <summary>
    /// The the reference to the specified spatial block.
    /// </summary>
    /// <param name="componentIndex">The index of the component.</param>
    /// <param name="blockX">The offset of blocks in the horizontal orientation.</param>
    /// <param name="blockY">The offset of blocks in the vertical orientation.</param>
    /// <returns>The reference to the block.</returns>
    public ref JpegBlock8x8 GetBlockReference(int componentIndex, int blockX, int blockY)
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

        ref JpegBlock8x8 blockRef = ref Unsafe.As<byte, JpegBlock8x8>(ref MemoryMarshal.GetReference(_bufferHandle!.Memory.Span));
        if (blockX >= component.HorizontalComponentBlock || blockY >= component.VerticalComponentBlock)
        {
            return ref blockRef;
        }

        return ref Unsafe.Add(ref blockRef, component.ComponentBlockOffset + blockY * component.HorizontalComponentBlock + blockX);
    }

    /// <summary>
    /// Flush the blocks into the specified <see cref="JpegBlockOutputWriter"/>.
    /// </summary>
    /// <param name="outputWriter">The output writer.</param>
    public void Flush(JpegBlockOutputWriter outputWriter)
    {
        if (outputWriter is null)
        {
            throw new ArgumentNullException(nameof(outputWriter));
        }

        ComponentAllocation[]? components = _components;

        if (components is null)
        {
            return;
        }

        for (int i = 0; i < components.Length; i++)
        {
            ComponentAllocation component = components[i];
            ref JpegBlock8x8 componentBlockRef = ref Unsafe.Add(ref Unsafe.As<byte, JpegBlock8x8>(ref MemoryMarshal.GetReference(_bufferHandle!.Memory.Span)), component.ComponentBlockOffset);

            for (int row = 0; row < component.VerticalComponentBlock; row++)
            {
                ref JpegBlock8x8 rowRef = ref Unsafe.Add(ref componentBlockRef, row * component.HorizontalComponentBlock);
                for (int col = 0; col < component.HorizontalComponentBlock; col++)
                {
                    ref JpegBlock8x8 blockRef = ref Unsafe.Add(ref rowRef, col);
                    WriteBlock(outputWriter, blockRef, i, col * component.HorizontalSubsamplingFactor * 8, row * component.VerticalSubsamplingFactor * 8, component.HorizontalSubsamplingFactor, component.VerticalSubsamplingFactor);
                }
            }
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

    /// <summary>
    /// Release all the resources allocated by the allocator.
    /// </summary>
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
        public int HorizontalComponentBlock;
        public int VerticalComponentBlock;
        public int ComponentBlockOffset;
    }
}
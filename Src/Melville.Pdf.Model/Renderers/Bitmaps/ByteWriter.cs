using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal interface IByteWriter
{
    unsafe void WriteBytes(scoped ref SequenceReader<byte> input, scoped ref byte* output, 
         byte* nextPos);
    int MinimumInputSize { get; }
}


internal abstract class ByteWriter: IByteWriter
{
    protected int MaxValue { get; }
    private readonly ComponentWriter componentWriter;
    private readonly int[] components;
    private int currentComponent;

    protected ByteWriter(int maxValue, ComponentWriter componentWriter)
    {
        MaxValue = maxValue;
        this.componentWriter = componentWriter;
        components = new int[componentWriter.ColorComponentCount];
    }
    
    public abstract unsafe void WriteBytes(
        scoped ref SequenceReader<byte> input, scoped ref byte* output, byte* nextPos);

    protected unsafe void PushComponent(ref byte* output, int numerator)
    {
        components[currentComponent++] = numerator;
        if (currentComponent >= components.Length)
        {
            var color = ComputeColor();
            PushPixel(ref output, color);
            currentComponent = 0;
        }
    }

    public static unsafe void PushPixel(ref byte* output, in DeviceColor color)
    {
        fixed (DeviceColor* colorptr = &color)
        {
            *((uint*)output) = *((uint*)colorptr);
            output += 3;
            *(output++) = 255;
        }
    }

    private const int maxSize = 10_000;

    private DeviceColor ComputeColor()
    {
        ClearCacheIfTooBig();
        return TryReadFromCache();
    }

    private DeviceColor TryReadFromCache()
    {
        ref DeviceColor cacheLine = ref
            CollectionsMarshal.GetValueRefOrAddDefault(cache, HashForComponents(), out bool exists);
        if (!exists)
        {
            cacheLine = componentWriter.ColorFromComponents(components);
        }

        return cacheLine;
    }

    private void ClearCacheIfTooBig()
    {
        if (cache.Count >= maxSize) cache.Clear();
    }

    private int HashForComponents()
    {
        var hc = new HashCode();
        foreach (var component in components)
        {
            hc.Add(component);
        }

        var hashCode = hc.ToHashCode();
        return hashCode;
    }

    private Dictionary<int, DeviceColor> cache = new(maxSize);

    public abstract int MinimumInputSize { get; }
}
using System;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.Colors;

internal partial class ColorSpaceCache : IColorSpace
{
    [DelegateTo] private readonly IColorSpace inner;
    private ColorSpaceCacheEntry[] cache;
    
    public ColorSpaceCache(IColorSpace inner, int size)
    {
        this.inner = inner;
        cache = new ColorSpaceCacheEntry[size];
        InitializeArray(inner.ExpectedComponents);
    }

    private void InitializeArray(int compomemts)
    {
        for (int i = 0; i < cache.Length; i++)
        {
            cache[i] = new ColorSpaceCacheEntry(compomemts);
        }
    }

    public DeviceColor SetColor(in ReadOnlySpan<double> newColor)
    {
        PlaceCorrectEntryInZeroSlot(newColor);
        return cache[0].Output;
    }

    private void PlaceCorrectEntryInZeroSlot(ReadOnlySpan<double> newColor)
    {
        for (int i = 0; i < cache.Length; i++)
        {
            if (cache[i].Matches(newColor))
            {
                RollToFirst(i);
                return;
            }
        }

        cache[^1].Remember(newColor, inner);
        RollToFirst(cache.Length - 1);
    }

    private void RollToFirst(int newFirstItem)
    {
        if (newFirstItem == 0) return;
        var saved = cache[newFirstItem];
        for (int j = newFirstItem; j > 0; j--)
        {
            cache[j] = cache[j - 1];
        }
        cache[0] = saved;
    }
}

internal class ColorSpaceCacheEntry
{
    public double[] Input { get; }
    public DeviceColor Output { get; private set; }

    public ColorSpaceCacheEntry(int size)
    {
        Input = new double[size];
        Input[0] = double.MinValue;
    }

    public bool Matches(in ReadOnlySpan<double> newColor) => newColor.SequenceEqual(Input);

    public void Remember(in ReadOnlySpan<double> color, IColorSpace inner)
    {
        color.CopyTo(Input);
        Output = inner.SetColor(color);
    }
}
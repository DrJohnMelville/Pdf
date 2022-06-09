using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public readonly struct SpacedStringContentBuilder
{
    private readonly List<ContentStreamValueUnion> items = new();
    public void Add(double item) => items.Add(new(item, (long)item));
    public void Add(in Memory<byte> item) => items.Add(new(item));
    public Span<ContentStreamValueUnion> GetAllValues() => CollectionsMarshal.AsSpan(items);

    public SpacedStringContentBuilder() 
    {
    }
}
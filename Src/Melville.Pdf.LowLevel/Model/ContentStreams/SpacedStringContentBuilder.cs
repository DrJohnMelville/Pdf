using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public readonly struct SpacedStringContentBuilder
{
    private readonly List<ContentStreamValueUnion> items = new();
    /// <summary>
    /// Add a double (or space) item to the spaced string
    /// </summary>
    /// <param name="item"></param>
    public void Add(double item) => items.Add(new(item, (long)item));
    /// <summary>
    /// Add a string to the spaced string
    /// </summary>
    /// <param name="item"></param>
    public void Add(in Memory<byte> item) => items.Add(new(item));
    /// <summary>
    /// Get the completed list of values as a span.
    /// </summary>
    /// <returns>A span containing the spaced string items.</returns>
    public Span<ContentStreamValueUnion> GetAllValues() => CollectionsMarshal.AsSpan(items);

    public SpacedStringContentBuilder() 
    {
    }
}
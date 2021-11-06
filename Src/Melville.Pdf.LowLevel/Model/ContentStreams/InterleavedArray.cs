using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public interface IInterleavedTarget<T1, T2>
{
    void Handle(T1 item);
    void Handle(T2 item);
}
public ref struct InterleavedArray<T1,T2>
{
    private readonly ReadOnlySpan<T1> items1;
    private readonly ReadOnlySpan<T2> items2;
    private readonly ReadOnlySpan<byte> order;

    public InterleavedArray(ReadOnlySpan<T1> items1, ReadOnlySpan<T2> items2, ReadOnlySpan<byte> order)
    {
        this.items1 = items1;
        this.items2 = items2;
        this.order = order;
    }

    // the generic construct here lets us use a struct target without boxing it.
    [Pure]
    public void Iterate<T>(T target) where T: IInterleavedTarget<T1,T2>
    {
        var t1Count = 0;
        var t2Count = 0;
        foreach (var picker in order)
        {
            if (picker == 1) 
                target.Handle(items1[t1Count++]);
            else
                target.Handle(items2[t2Count++]); 
        }
    }
}

public readonly struct InterleavedArrayBuilder
{
    private readonly List<ContentStreamValueUnion> items = new();
    public void Handle(double item) => items.Add(new(item, (long)item));
    public void Handle(in Memory<byte> item) => items.Add(new(item));
    public Span<ContentStreamValueUnion> GetInterleavedArray() => CollectionsMarshal.AsSpan(items);
}
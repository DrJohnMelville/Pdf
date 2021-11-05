using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

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

public class InterleavedArrayBuilder<T1, T2>
{
    private List<T1> t1s = new();
    private List<T2> t2s = new();
    private List<byte> order = new();
    public void Handle(T1 item)
    {
        order.Add(1);
        t1s.Add(item);
    }

    public void Handle(T2 item)
    {
        order.Add(2);
        t2s.Add(item);
    }

    public T1 GetT1(int position) => t1s[position];
    public T2 GetT2(int position) => t2s[position];

    public void Clear()
    {
        t1s.Clear();
        t2s.Clear();
        order.Clear();
    }

    public InterleavedArray<T1, T2> GetInterleavedArray() => new(
        CollectionsMarshal.AsSpan(t1s), CollectionsMarshal.AsSpan(t2s), CollectionsMarshal.AsSpan(order));
}
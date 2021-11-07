using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

public readonly struct ContentStreamValueStack
{
    private readonly List<ContentStreamValueUnion> values = new();
    public int Count => values.Count;
    
    public void Add(object item) => values.Add(new ContentStreamValueUnion(item));
    public void Add(in Memory<byte> item) => values.Add(new ContentStreamValueUnion(item));
    public void Add(double floating, long integer) => 
        values.Add(new ContentStreamValueUnion(floating, integer));

    public void Clear() => values.Clear();

    public double DoubleAt(int x) => values[x].Floating;
    public long LongAt(int x) => values[x].Integer;
    public Memory<byte> BytesAt(int x) => values[x].Bytes;
    public ContentStreamValueType TypeAt(int x) => values[x].Type;
    public T ObjectAt<T>(int x)  where T: class => values[x].Object as T ??
                                                   throw new PdfParseException("Wrong type in Content Stream Parser");

    public PdfName NamaAt(int x) => ObjectAt<PdfName>(x);

    public void FillSpan(in Span<double> target)
    {
        for (int i = 0; i < target.Length; i++)
        {
            target[i] = values[i].Floating;
        }
    }

    public Span<ContentStreamValueUnion> NativeSpan() => CollectionsMarshal.AsSpan(values);
}
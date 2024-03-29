﻿using System;
using System.Buffers;
using System.Text;

namespace Melville.Pdf.LowLevel.Model.ShortStrings;

internal interface IShortString
{
    int ComputeHashCode();
    int Length();
    bool SameAs(in ReadOnlySpan<byte> other);
    string ValueAsString();
    public bool CheckReaderFor(ref SequenceReader<byte> input, bool final, out bool result);
    void Fill(Span<byte> span);
}

internal readonly struct ShortString<T> : IShortString where T : IPackedBytes
{
    private readonly T data;

    public ShortString(T data)
    {
        this.data = data;
    }

    public int ComputeHashCode()
    {
        var hash = new FnvComputer();
        data.AddToHash(ref hash);
        return hash.HashAsInt();
    }

    public int Length() => data.Length();

    public bool SameAs(in ReadOnlySpan<byte> other) =>
        data.SameAs(other);

    public string ValueAsString()
    {
        Span<byte> chars = stackalloc byte[data.Length()];
        data.Fill(chars);
        return Encoding.UTF8.GetString(chars);
    }

    public bool CheckReaderFor(ref SequenceReader<byte> input, bool final, out bool result)
    {
        var len = data.Length();
        Span<byte> inputSpan = stackalloc byte[len];
        if (!input.TryCopyTo(inputSpan))
        {
            result = false;
            return final;
        }
        input.Advance(len);
        result = data.SameAs(inputSpan);
        return true;
    }

    public void Fill(Span<byte> span) => data.Fill(span);
}
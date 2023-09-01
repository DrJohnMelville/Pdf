using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Microsoft.CodeAnalysis.Emit;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

internal readonly partial struct CMapFactory
{
    [FromConstructor] private readonly IList<ByteRange> data;

    public void AddCodespaces(ReadOnlySpan<PostscriptValue> values) => 
        values.ForEachGroup(AddSingleCodespace);

    private void AddSingleCodespace(PostscriptValue minValue, PostscriptValue maxValue) => 
        data.Add(new ByteRange(ToVbc(minValue), ToVbc(maxValue)));

    private VariableBitChar ToVbc(in PostscriptValue value) => 
        new(value.Get<StringSpanSource>().GetSpan());

    public void AddNotDefRanges(ReadOnlySpan<PostscriptValue> values) =>
        values.ForEachGroup(AddSingleNotDefRange);

    private void AddSingleNotDefRange(
        PostscriptValue min, PostscriptValue max, PostscriptValue value) => 
        AddRange(new ConstantCMapper(ToVbc(min), ToVbc(max), (uint)value.Get<long>()));

    public void AddCidRanges(ReadOnlySpan<PostscriptValue> values) =>
        values.ForEachGroup(AddSingleCidRange);

    private void AddSingleCidRange(
        PostscriptValue min, PostscriptValue max, PostscriptValue value) => 
        AddRange(new LinearCMapper(ToVbc(min), ToVbc(max), (uint)value.Get<long>()));

    public void AddCidChars(ReadOnlySpan<PostscriptValue> values) =>
        values.ForEachGroup(AddSingleCidChar);

    private void AddSingleCidChar(PostscriptValue source, PostscriptValue dest)
    {
        var sourceVbc = ToVbc(source);
        AddRange(new ConstantCMapper(sourceVbc, sourceVbc, (uint)dest.Get<long>()));
    }


    private void AddRange(CMapMapperBase mapper)
    {
        foreach (var range in data)
        {
            if (range.Contains(mapper))
            {
                range.AddMapper(mapper);
            }
        }
    }
}

public static class SpanChunker
{
    public static void ForEachGroup<T>(
        this ReadOnlySpan<T> values, Action<T, T> body)
    {
        Debug.Assert(values.Length % 2 == 0);
        for (int i = 0; i < values.Length; i+=2)
        {
            body(values[i],values[i + 1]);
        }
        
    }
    public static void ForEachGroup<T>(
        this ReadOnlySpan<T> values, Action<T, T, T> body)
    {
        Debug.Assert(values.Length % 3 == 0);
        for (int i = 0; i < values.Length; i+=3)
        {
            body(values[i],values[i + 1],values[i + 2]);
        }
        
    }
}
using System;
using System.Net.Http.Headers;

namespace Melville.Pdf.LowLevel.Model.ShortStrings;

internal interface IShortStringTarget<T>
{
    T Create(ShortString<NinePackedBytes> data);
    T Create(ShortString<EighteenPackedBytes> data);
    T Create(ShortString<ArbitraryBytes> data);
}

internal static class ShortStringFactory
{
    public static T WrapWith<T>(this in ReadOnlySpan<byte> data, IShortStringTarget<T> target)
    {
        return ReportLongLengthIfExtendedCharactersPresent<T>(data) switch
        {
            < 10 => target.Create(new ShortString<NinePackedBytes>(new(data))),
            < 19 => target.Create(new ShortString<EighteenPackedBytes>(new(data))),
            _=> target.Create(new ShortString<ArbitraryBytes>(new(data)))
        };
    }

    private static int ReportLongLengthIfExtendedCharactersPresent<T>(ReadOnlySpan<byte> data)
    {
        foreach (var datum in data)
        {
            if (datum > 127) return int.MaxValue;
        }
        return data.Length;
    }
}
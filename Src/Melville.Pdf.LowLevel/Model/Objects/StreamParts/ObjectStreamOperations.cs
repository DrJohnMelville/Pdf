using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.PipeReaders;
using Melville.Pdf.LowLevel.Filters.LzwFilter;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Model.Objects.StreamParts;

internal static class ObjectStreamOperations
{
    public static async ValueTask ReportIncludedObjectsAsync(
        this PdfStream stream, InternalObjectTargetForStream target)
    {
        await using var decoded = await stream.StreamContentAsync().CA();
        var bytes = ReusableStreamByteSource.Rent(decoded, false);

        await ReportIncludedObjectsAsync(stream, target, bytes).CA();
    }

    public static async Task ReportIncludedObjectsAsync(
        PdfStream stream, InternalObjectTargetForStream target, IByteSource bytes)
    {
        var firstObjectOffset = await stream.GetAsync<int>(KnownNames.First).CA();
        var buffer = ArrayPool<byte>.Shared.Rent(firstObjectOffset);
        var result = await bytes.ReadAtLeastAsync(firstObjectOffset).CA();
        result.Buffer.Slice(0, firstObjectOffset).CopyTo(buffer);
        bytes.AdvanceTo(result.Buffer.GetPosition(firstObjectOffset));

        var numbers = buffer.AsMemory(0, firstObjectOffset);
        for (var i = 0;; i++)
        {
            var objNum = ParseNumber(ref numbers);
            if (objNum < 0) return;
            var offset = ParseNumber(ref numbers);
            if (offset < 0) return;
            await target.ReportObjectAsync(objNum, i, offset + firstObjectOffset).CA();
        }
    }

    private static int ParseNumber(ref Memory<byte> sourceMemory)
    {
        var source = sourceMemory.Span;
        var consumed = source.IndexOfAny(digits);
        if (consumed < 0) return -1;
        NumberTokenizer.TryGetDigitSequence(10, source[consumed..],
                out var value, out var digitsLength);
        sourceMemory = sourceMemory.Slice(consumed + digitsLength);
        return (int)value;
    }

    private static ReadOnlySpan<byte> digits => "0123456789"u8;
}
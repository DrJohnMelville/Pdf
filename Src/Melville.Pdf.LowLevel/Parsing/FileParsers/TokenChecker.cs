using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

internal static class TokenChecker
{
    public static async ValueTask<bool> CheckTokenAsync(IByteSourceWithGlobalPosition reader, byte[] template)
    {
        bool result = false;
        do
        {
        } while (reader.ShouldContinue(VerifyTag(await reader.ReadAsync().CA(), template, out result)));

        return result;
    }

    private static (bool Success, SequencePosition Position) VerifyTag(
        ReadResult source, in ReadOnlySpan<byte> template, out bool result)
    {
        var reader = new SequenceReader<byte>(source.Buffer);
        return (reader.TryCheckToken(template, source.IsCompleted, out result), reader.Position);
    }

    public static bool TryCheckToken(this ref SequenceReader<byte> input, in ReadOnlySpan<byte> template, bool final,
        out bool result)
    {
        result = false;
        Span<byte> buffer = stackalloc byte[template.Length];
        if (!input.TryCopyTo(buffer)) return false;
        if (!buffer.SequenceEqual(template)) return true;
        input.Advance(template.Length);
        result = true;
        return true;
    }
}
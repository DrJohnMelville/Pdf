using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;
using System.Security.AccessControl;

namespace Melville.Parsing;

/// <summary>
/// This is a temporary bridge class
/// </summary>
[Obsolete("This needs to go into Melville.StreamInterfaces")]
public static class StreamExtensnions
{
    extension(ParsingBuffer pb)
    {
        public async ValueTask SkipToAsync(SearchValues<byte> chars) =>
            pb.Advance(await pb.EnsureHasCharAsync(chars));
        public async ValueTask<int> EnsureHasCharAsync(SearchValues<byte> chars)
        {
            int index;
            do
            {
                index = pb.Peek().IndexOfAny(chars);
            } while (index < 0 && (await pb.ReadBytesAsync().CA()) > 0);
            return index;
        }
        public async ValueTask SkipOverAsync(SearchValues<byte> chars) =>
            pb.Advance(await pb.EnsureHasNonCharAsync(chars));
        public async ValueTask<int> EnsureHasNonCharAsync(SearchValues<byte> chars)
        {
            int index;
            do
            {
                index = pb.Peek().IndexOfAnyExcept(chars);
            } while (index < 0 && (await pb.ReadBytesAsync().CA()) > 0);
            return index;
        }
    }

    extension(ref SpanParsingBuffer self)
    {
    }
}

public static class Force
{
    public static IDisposable Using(object? source) => source as IDisposable ?? FakeDisposable;

    public static IDisposable Using<T>(T source, out T reflected)
    {
        reflected = source;
        return Using(source);
    }

    private static IDisposable FakeDisposable = new FakeDisposeImpl();
    private class FakeDisposeImpl: IDisposable
    {
        public void Dispose() { }
    }
}
using Melville.INPC;

namespace Melville.Parsing.MultiplexSources;

internal interface ICountedSource
{

    private static int nextNonce= 0;
    public static int NextNonce() => Interlocked.Increment(ref nextNonce);

    bool TryRelease(ref CountedSourceTicket ticket);
}

internal readonly partial struct CountedSourceTicket
{
    [FromConstructor]private readonly ICountedSource? source;
    [FromConstructor]private readonly int nonce;

    public bool TryRelease(ref CountedSourceTicket ticket) => source?.TryRelease(ref ticket) ?? false;
    public bool HasNonce(int candidate) => nonce == candidate;

    public bool IsEmpty => source is null;
}

internal static class CountedSourceTicketOperations
{
    public static bool TryRelease(this ref CountedSourceTicket ticket) => ticket.TryRelease(ref ticket);
}
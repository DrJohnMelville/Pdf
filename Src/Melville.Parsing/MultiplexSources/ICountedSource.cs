using Melville.INPC;

namespace Melville.Parsing.MultiplexSources;

internal interface IHasSourceTicket
{
    CountedSourceTicket SourceTicket { get; set; }
}

internal interface ICountedSource
{

    private static int nextNonce= 0;
    public static int NextNonce() => Interlocked.Increment(ref nextNonce);

    void TryRelease(IHasSourceTicket item);
}

internal readonly partial struct CountedSourceTicket
{
    [FromConstructor]private readonly ICountedSource? source;
    [FromConstructor]private readonly int nonce;

    public void Release(IHasSourceTicket item) => source?.TryRelease(item);
    public bool HasNonce(int candidate) => nonce == candidate;

    public bool IsEmpty => source is null;
}


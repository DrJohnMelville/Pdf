namespace Melville.Parsing.Streams;

internal class MultiBufferNode
{
    public byte[] Data { get; }
    public long InitialPosition { get; }
    private MultiBufferNode? Next { get; set; }
    private long EndPosition => InitialPosition + Data.Length;

    public MultiBufferNode(byte[] data, long initialPosition)
    {
        this.Data = data;
        InitialPosition = initialPosition;
        Next = null;
    }

    public MultiBufferNode ForceNextNode() => 
        Next ??= new MultiBufferNode(new byte[Data.Length], EndPosition);

    public MultiBufferNode TryNextNode() => Next ?? throw new InvalidOperationException("Walked off end of buffer");

    public MultiBufferPosition FindPosition(long position)
    {
        if (position < InitialPosition)
            throw new ArgumentException("Cannot find a position less than current node");
        var node = this;
        while (node.Next is { } next && next.InitialPosition < position) node = next;
        return new MultiBufferPosition(node, position);
    }
}
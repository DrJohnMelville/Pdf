namespace Melville.Parsing.Streams;

internal readonly struct MultiBufferPosition
{
    public MultiBufferNode Node { get; }
    public long StreamPosition { get; }

    public MultiBufferPosition(MultiBufferNode node, long streamPosition)
    {
        Node = node;
        StreamPosition = streamPosition;
    }
}
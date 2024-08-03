namespace Melville.Parsing.LinkedLists
{
    internal class MultiBufferStreamList : LinkedList<MultiBufferStreamList>
    {
        public static LinkedList WritableList(int blockSize) =>
            new MultiBufferStreamList().With(blockSize);
        public static LinkedList SingleItemList(ReadOnlyMemory<byte> source) =>
            new MultiBufferStreamList().With(source);

    }
}
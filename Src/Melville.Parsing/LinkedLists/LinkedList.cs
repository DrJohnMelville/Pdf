using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Parsing.LinkedLists;

internal readonly struct LinkedListBehavior
{

}

internal class LinkedList
{
    private LinkedListBehavior behavior = default;
    private LinkedListPosition startPosition = LinkedListPosition.NullPosition;
    private LinkedListPosition endPosition = LinkedListPosition.NullPosition;
    private int references = 0;
    private int blockSize;
}
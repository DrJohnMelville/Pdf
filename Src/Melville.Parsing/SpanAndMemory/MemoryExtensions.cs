namespace Melville.Parsing.SpanAndMemory;

public static class MemoryExtensions
{
    public static T At<T>(this ReadOnlyMemory<T> mem, int position) => mem.Span[position];
}
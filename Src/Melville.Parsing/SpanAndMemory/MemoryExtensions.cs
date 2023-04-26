namespace Melville.Parsing.SpanAndMemory;

/// <summary>
/// Extension method on Memory
/// </summary>
public static class MemoryExtensions
{
    /// <summary>
    /// Return the nth element of the read only memory.
    /// </summary>
    /// <typeparam name="T">Element type of the memory</typeparam>
    /// <param name="mem">The source Memory&lt;T&gt;</param>
    /// <param name="position">Index of the item to retrieve.</param>
    /// <returns>The position'th element of mem</returns>
    public static T At<T>(this ReadOnlyMemory<T> mem, int position) => mem.Span[position];
}
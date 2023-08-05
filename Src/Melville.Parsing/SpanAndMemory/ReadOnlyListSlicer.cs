using System.Collections;
using Melville.INPC;

namespace Melville.Parsing.SpanAndMemory;

/// <summary>
/// Implements a slice operator on IReadOnlyList
/// </summary>
public static class ReadOnlyListSlicer
{
    /// <summary>
    /// Slice a IReadOnlyList, returning a wrapper containing only the specified range.
    /// </summary>
    /// <typeparam name="T">The component type of the IReadOnlyList</typeparam>
    /// <param name="list">The source list.</param>
    /// <param name="range">The range of items to include</param>
    /// <returns>A wrapper around the original list than only contains the given range of items</returns>
    public static IReadOnlyList<T> Slice<T>(this IReadOnlyList<T> list, Range range)
    {
        var sourceLength = list.Count;
        return list.Slice(range.Start.GetOffset(sourceLength), range.End.GetOffset(sourceLength));
    }

    /// <summary>
    /// Slice a IReadOnlyList, returning a wrapper containing a starting element to the end.
    /// </summary>
    /// <typeparam name="T">The component type of the IReadOnlyList</typeparam>
    /// <param name="list">The source list.</param>
    /// <param name="start">The source index of the first element in the new span</param>
    /// <returns>A wrapper around the original list than only contains the given range of items</returns>
    public static IReadOnlyList<T> Slice<T>(this IReadOnlyList<T> list, int start) =>
        list.Slice(start, list.Count - start);

    /// <summary>
    /// Slice a IReadOnlyList, returning a wrapper containing a starting element to the end.
    /// </summary>
    /// <typeparam name="T">The component type of the IReadOnlyList</typeparam>
    /// <param name="list">The source list.</param>
    /// <param name="start">The source index of the first element in the new span</param>
    /// <param name="length">The length of the resulting sliced list</param>
    /// <returns>A wrapper around the original list than only contains the given range of items</returns>
    public static IReadOnlyList<T> Slice<T>(this IReadOnlyList<T> list, int start, int length) =>
        new SlicedReadOnlyList<T>(list, start, length);
}

internal sealed partial class SlicedReadOnlyList<T> : IReadOnlyList<T>
{
    [FromConstructor] private readonly IReadOnlyList<T> list;
    [FromConstructor] private readonly int start;
    [FromConstructor] public int Count { get; }

    partial void OnConstructed()
    {
        if (start < 0 || Count < 0 || start + Count > list.Count)
            throw new InvalidOperationException("Source list does not contain entire slice");
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<T> GetEnumerator()
    {
        for (int i = start; i < start+Count; i++)
        {
            yield return list[i];
        }
    }

    public T this[int index] => list[index + start];
}
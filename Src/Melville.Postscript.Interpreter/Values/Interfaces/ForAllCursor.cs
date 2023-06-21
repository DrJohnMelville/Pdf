using System;
using System.Diagnostics;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values
{
    public readonly partial struct ForAllCursor
    {
        /// <summary>
        /// A Memory containing the items to be iterated over.
        /// </summary>
        [FromConstructor] private readonly Memory<PostscriptValue> items;
        /// <summary>
        /// Indicates the number of items pushed per iteration.
        /// </summary>
        [FromConstructor] public int ItemsPer { get; }

        partial void OnConstructed()
        {
            Debug.Assert(items.Length % ItemsPer == 0);
        }

        /// <summary>
        /// Try to do another iteration of the ForAll operation.
        /// </summary>
        /// <param name="engine">Postscript engine we are running on</param>
        /// <param name="index">A scratch value that starts at 0 and the object uses
        /// as iteration state.</param>
        /// <returns></returns>
        public bool TryGetItr(in Span<PostscriptValue> target, ref int index)
        {
            if (index >= items.Length) return false;
            items.Span.Slice(index, ItemsPer).CopyTo(target);
            index += ItemsPer;
            return true;
        }
    }
}
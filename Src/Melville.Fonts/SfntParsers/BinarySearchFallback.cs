namespace Melville.Fonts.SfntParsers
{
    internal static class BinarySearchFallback
    {
        public static int BinarySearchWithFallback<T>(this Span<T> sp, IComparable<T> key) =>
            BinarySearchWithFallback((ReadOnlySpan<T>)sp, key);

        public static int BinarySearchWithFallback<T>(this ReadOnlySpan<T> sp, IComparable<T> key)
        {
            var ret = sp.BinarySearch(key);
            if (ret > 0) return ret;
            for (int i = 0; i < sp.Length; i++)
            {
                if (key.CompareTo(sp[i]) == 0)
                    return i;
            }

            return -1;
        }
    }
}
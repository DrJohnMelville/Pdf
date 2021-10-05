using System;
using System.Collections.Generic;
using System.Linq;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public static class HexStrings
    {
        public static string AsHex(this byte[] str) =>
            ((IEnumerable<byte>)str).AsHex();
        public static string AsHex(this IEnumerable<byte> str) =>
            string.Join(" ", str.Select(i => i.ToString("X2")));
        public static string AsHex(in this ReadOnlySpan<byte> str) =>
            string.Join(" ", str.ToArray().Select(i => i.ToString("X2")));
    }
}
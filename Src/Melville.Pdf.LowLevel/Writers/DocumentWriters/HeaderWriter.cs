using System;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters
{
    public static class HeaderWriter
    {

        public static void WriteHeader(PipeWriter tar, byte majorVersion, byte minorVersion)
        {
            var dest = tar.GetSpan(header.Length);
            header.AsSpan().CopyTo(dest);
            FixVersionElement(dest, majorVersion, 5);
            FixVersionElement(dest, minorVersion, 7);
            tar.Advance(header.Length);
        }

        private static void FixVersionElement(Span<byte> dest, byte minorVersion, int versionByteIndex)
        {
            if (minorVersion > 9) throw new ArgumentException("Major and minor version must be 1-9");
            dest[versionByteIndex] = HexMath.HexDigits[minorVersion];
        }

        private static readonly byte[] header = 
            {37, 80, 68, 70, 45, 49, 46, 55, 13, 10, 37, 255, 255, 255, 255, 32, 67, 114, 101, 97, 116, 101, 100, 32, 119, 105, 116, 104, 32, 77, 101, 108, 118, 105, 108, 108, 101, 46, 80, 100, 102};
        // %PDF-1.7%ÿÿÿÿ Created with Melville.Pdf
        
    }
}
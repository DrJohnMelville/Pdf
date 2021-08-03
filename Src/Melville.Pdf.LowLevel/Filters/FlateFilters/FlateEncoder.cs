using System;
using System.IO;
using System.IO.Compression;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FlateFilters
{
    public class FlateEncoder:IEncoder
    {
        private static byte[] prefix = {0x78, 0xDA};
        public byte[] Encode(byte[] data, PdfObject? parameters)
        {
            var ret = new MemoryStream();
            ret.Write(prefix, 0, 2);
            var deflate = new DeflateStream(ret, CompressionLevel.Optimal);
            deflate.Write(data);
            deflate.Flush();
            var adler = new Adler32Computer(1);
            adler.AddData(data);
            var checksum = adler.GetHash();
             ret.WriteByte((byte)(checksum >> 24));
             ret.WriteByte((byte)(checksum >> 16));
             ret.WriteByte((byte)(checksum >> 8));
             ret.WriteByte((byte)(checksum));
            return ret.ToArray();
        }
    }

    public ref struct Adler32Computer
    {
        private ulong s1;
        private ulong s2;

        public Adler32Computer(uint priorAdler = 1)
        {
            s1 = priorAdler & 0xFFFF;
            s2 = (priorAdler >> 16) & 0xFFFF;
        }
        private const ulong AdlerBase = 65521; /* largest prime smaller than 65536 */

        public void AddData(Span<byte> bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                s1 += bytes[i];
                s1 %= AdlerBase;
                s2 += s1;
                s2 %= AdlerBase;
            }
        }

        public uint GetHash() =>(uint) ((s2 << 16) | s1);
    }
}
using System.IO;
using System.IO.Compression;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FlateFilters
{
    public class FlateEncoder:IEncoder
    {
        public byte[] Encode(byte[] data, PdfObject? parameters)
        {
            var ret = new MemoryStream();
            WritePrefix(ret);
            WriteCompressedDate(data, ret);
            WriteAdlerHash(ret, ComputeChecksum(data));
            return ret.ToArray();
        }

        private static uint ComputeChecksum(byte[] data)
        {
            var adler = new Adler32Computer(1);
            adler.AddData(data);
            var checksum = adler.GetHash();
            return checksum;
        }

        private static byte[] prefix = {0x78, 0xDA};
        private static void WritePrefix(MemoryStream? ret) => ret.Write(prefix, 0, 2);

        private static void WriteCompressedDate(byte[] data, MemoryStream ret)
        {
            using var deflate = new DeflateStream(ret, CompressionLevel.Optimal, leaveOpen:true);
            deflate.Write(data);
            deflate.Flush();
        }

        private static void WriteAdlerHash(MemoryStream ret, uint checksum)
        {
            ret.WriteByte((byte) (checksum >> 24));
            ret.WriteByte((byte) (checksum >> 16));
            ret.WriteByte((byte) (checksum >> 8));
            ret.WriteByte((byte) (checksum));
        }
    }
}
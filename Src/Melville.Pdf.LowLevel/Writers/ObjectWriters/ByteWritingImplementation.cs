using System.IO.Pipelines;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters
{
    public static class ByteWritingImplementation
    {
        public static void WriteSpace(this PipeWriter dest) => dest.WriteByte((byte)' ');
        public static void WriteLineFeed(this PipeWriter dest) => dest.WriteByte((byte)'\n');
        public static void WriteByte(this PipeWriter dest, byte b)
        {
            var span = dest.GetSpan(1);
            span[0] = b;
            dest.Advance(1);
        }
        public static void WriteBytes(this PipeWriter dest, byte b, byte b2)
        {
            var span = dest.GetSpan(2);
            span[0] = b;
            span[1] = b2;
            dest.Advance(2);
        }
    }
}
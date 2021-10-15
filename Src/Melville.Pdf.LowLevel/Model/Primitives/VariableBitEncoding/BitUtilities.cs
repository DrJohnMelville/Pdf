namespace Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding
{
    public static class BitUtilities
    {
        public static byte Mask(int bits) => (byte) ((1 << bits) - 1);
    }
}
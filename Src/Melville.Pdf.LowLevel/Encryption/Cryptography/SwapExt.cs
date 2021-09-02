namespace Melville.Pdf.LowLevel.Encryption.Cryptography
{
    static class SwapExt
    {
        public static void Swap<T>(this T[] array, int index1, int index2) => 
            (array[index1], array[index2]) = (array[index2], array[index1]);
    }
}
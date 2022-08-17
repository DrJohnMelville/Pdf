namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public enum JpegBlockType
{
    StartOfImage = 0xFFD8,
    ApplicationDefaultHeader = 0xFFE0,
    QuantizationTable=0xFFDB,
    StartOfFrame = 0xFFC0,
    DefineHuffmanTable = 0xFFC4,
    StartOfScan = 0xFFDA,
    EndOfImage = 0xFFd9
}
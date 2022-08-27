using Melville.JpegLibrary.Quantization;

namespace Melville.JpegLibrary.ArithmeticDecoding;

internal class JpegArithmeticDecodingComponent
{
    public int ComponentIndex { get; internal set; }
    public byte HorizontalSamplingFactor { get; internal set; }
    public byte VerticalSamplingFactor { get; internal set; }
    internal int DcPredictor { get; set; }
    internal JpegArithmeticDecodingTable? DcTable { get; set; }
    internal JpegArithmeticDecodingTable? AcTable { get; set; }
    internal JpegQuantizationTable QuantizationTable { get; set; }
    internal int HorizontalSubsamplingFactor { get; set; }
    internal int VerticalSubsamplingFactor { get; set; }

    internal int DcContext { get; set; }

    internal JpegArithmeticStatistics? DcStatistics { get; set; }
    internal JpegArithmeticStatistics? AcStatistics { get; set; }
}
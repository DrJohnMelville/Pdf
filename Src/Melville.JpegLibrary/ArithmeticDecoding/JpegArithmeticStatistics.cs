namespace Melville.JpegLibrary.ArithmeticDecoding;

internal class JpegArithmeticStatistics
{
    private readonly bool _dc;
    private readonly byte _identifier;
    private readonly byte[] _statistics;

    public JpegArithmeticStatistics(bool dc, byte identifier)
    {
        _dc = dc;
        _identifier = identifier;
        _statistics = dc ? new byte[64] : new byte[256];
    }

    public bool IsDcStatistics => _dc;

    public byte Identifier => _identifier;

    public ref byte GetReference() => ref _statistics[0];

    public ref byte GetReference(int offset) => ref _statistics[offset];

    public void Reset()
    {
        _statistics.AsSpan().Clear();
    }
}

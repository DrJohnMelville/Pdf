namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

public readonly struct QeEntry
{
    public ushort Qe { get; }
    public byte NMPS { get; }
    public byte NLPS { get; }
    public bool Switch { get; }

    public QeEntry(ushort qe, byte nmps, byte nlps, bool @switch)
    {
        Qe = qe;
        NMPS = nmps;
        NLPS = nlps;
        Switch = @switch;
    }

    public void TrySwitch(ref ContextEntry currentState)
    {
        if (!Switch) return;
        currentState.MPS = (byte)(1 - currentState.MPS);
    }
}

public static class QeComputer
{
    public static readonly QeEntry[] Rows =
    {
        new(0x5601, 1, 1, true),
        new(0x3401, 2, 6, false),
        new(0x1801, 3, 9, false),
        new(0x0AC1, 4, 12, false),
        new(0x0521, 5, 29, false),
        new(0x0221, 38, 33, false),
        new(0x5601, 7, 6, true),
        new(0x5401, 8, 14, false),
        new(0x4801, 9, 14, false),
        new(0x3801, 10, 14, false),
        new(0x3001, 11, 17, false),
        new(0x2401, 12, 18, false),
        new(0x1C01, 13, 20, false),
        new(0x1601, 29, 21, false),
        new(0x5601, 15, 14, true),
        new(0x5401, 16, 14, false),
        new(0x5101, 17, 15, false),
        new(0x4801, 18, 16, false),
        new(0x3801, 19, 17, false),
        new(0x3401, 20, 18, false),
        new(0x3001, 21, 19, false),
        new(0x2801, 22, 19, false),
        new(0x2401, 23, 20, false),
        new(0x2201, 24, 21, false),
        new(0x1C01, 25, 22, false),
        new(0x1801, 26, 23, false),
        new(0x1601, 27, 24, false),
        new(0x1401, 28, 25, false),
        new(0x1201, 29, 26, false),
        new(0x1101, 30, 27, false),
        new(0x0AC1, 31, 28, false),
        new(0x09C1, 32, 29, false),
        new(0x08A1, 33, 30, false),
        new(0x0521, 34, 31, false),
        new(0x0441, 35, 32, false),
        new(0x02A1, 36, 33, false),
        new(0x0221, 37, 34, false),
        new(0x0141, 38, 35, false),
        new(0x0111, 39, 36, false),
        new(0x0085, 40, 37, false),
        new(0x0049, 41, 38, false),
        new(0x0025, 42, 39, false),
        new(0x0015, 43, 40, false),
        new(0x0009, 44, 41, false),
        new(0x0005, 45, 42, false),
        new(0x0001, 45, 43, false),
        new(0x5601, 46, 46, false),
    };
}
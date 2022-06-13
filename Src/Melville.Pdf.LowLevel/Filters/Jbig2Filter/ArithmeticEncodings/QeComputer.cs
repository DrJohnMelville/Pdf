namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

public readonly struct QeEntry
{
    public uint Qe { get; }
    public byte NMPS { get; }
    public byte NLPS { get; }
    private readonly bool shouldSwitch;

    public QeEntry(uint qe, byte nmps, byte nlps, bool shouldSwitch)
    {
        Qe = qe;
        NMPS = nmps;
        NLPS = nlps;
        this.shouldSwitch = shouldSwitch;
    }

    public void TrySwitch(ref ContextEntry currentState)
    {
        if (shouldSwitch) currentState.InvertMPS();
    }
}

public static class QeComputer
{
    public static readonly QeEntry[] Rows =
    {
        new(0x56010000, 1, 1, true),
        new(0x34010000, 2, 6, false),
        new(0x18010000, 3, 9, false),
        new(0x0AC10000, 4, 12, false),
        new(0x05210000, 5, 29, false),
        new(0x02210000, 38, 33, false),
        new(0x56010000, 7, 6, true),
        new(0x54010000, 8, 14, false),
        new(0x48010000, 9, 14, false),
        new(0x38010000, 10, 14, false),
        new(0x30010000, 11, 17, false),
        new(0x24010000, 12, 18, false),
        new(0x1C010000, 13, 20, false),
        new(0x16010000, 29, 21, false),
        new(0x56010000, 15, 14, true),
        new(0x54010000, 16, 14, false),
        new(0x51010000, 17, 15, false),
        new(0x48010000, 18, 16, false),
        new(0x38010000, 19, 17, false),
        new(0x34010000, 20, 18, false),
        new(0x30010000, 21, 19, false),
        new(0x28010000, 22, 19, false),
        new(0x24010000, 23, 20, false),
        new(0x22010000, 24, 21, false),
        new(0x1C010000, 25, 22, false),
        new(0x18010000, 26, 23, false),
        new(0x16010000, 27, 24, false),
        new(0x14010000, 28, 25, false),
        new(0x12010000, 29, 26, false),
        new(0x11010000, 30, 27, false),
        new(0x0AC10000, 31, 28, false),
        new(0x09C10000, 32, 29, false),
        new(0x08A10000, 33, 30, false),
        new(0x05210000, 34, 31, false),
        new(0x04410000, 35, 32, false),
        new(0x02A10000, 36, 33, false),
        new(0x02210000, 37, 34, false),
        new(0x01410000, 38, 35, false),
        new(0x01110000, 39, 36, false),
        new(0x00850000, 40, 37, false),
        new(0x00490000, 41, 38, false),
        new(0x00250000, 42, 39, false),
        new(0x00150000, 43, 40, false),
        new(0x00090000, 44, 41, false),
        new(0x00050000, 45, 42, false),
        new(0x00010000, 45, 43, false),
        new(0x56010000, 46, 46, false),
    };
}
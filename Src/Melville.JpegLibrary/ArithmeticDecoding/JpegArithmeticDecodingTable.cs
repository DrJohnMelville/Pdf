using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Melville.JpegLibrary.ArithmeticDecoding;

internal class JpegArithmeticDecodingTable
{
    public JpegArithmeticDecodingTable(byte tableClass, byte identifier)
    {
        TableClass = tableClass;
        Identifier = identifier;
    }

    public byte TableClass { get; }
    public byte Identifier { get; }

    public void Configure(byte conditioningTableValue)
    {
        ConditioningTableValue = conditioningTableValue;
        if (TableClass == 0)
        {
            DcL = conditioningTableValue & 0x0f;
            DcU = conditioningTableValue >> 4;
            AcKx = 0;
        }
        else
        {
            DcL = 0;
            DcU = 0;
            AcKx = conditioningTableValue;
        }
    }

    public byte ConditioningTableValue { get; private set; }

    public int DcL { get; private set; }
    public int DcU { get; private set; }
    public int AcKx { get; private set; }

    public static bool TryParse(ReadOnlySequence<byte> buffer, [NotNullWhen(true)] out JpegArithmeticDecodingTable? arithmeticTable, out int bytesConsumed)
    {
#if NO_READONLYSEQUENCE_FISTSPAN
            ReadOnlySpan<byte> firstSpan = buffer.First.Span;
#else
        ReadOnlySpan<byte> firstSpan = buffer.FirstSpan;
#endif

        if (firstSpan.Length >= 2)
        {
            return TryParse(firstSpan, out arithmeticTable, out bytesConsumed);
        }

        bytesConsumed = 0;
        if (firstSpan.IsEmpty)
        {
            arithmeticTable = null;
            return false;
        }

        byte tableClassAndIdentifier = firstSpan[0];
        bytesConsumed++;

        return TryParse((byte)(tableClassAndIdentifier >> 4), (byte)(tableClassAndIdentifier & 0xf), buffer.Slice(1), out arithmeticTable, ref bytesConsumed);

    }

    public static bool TryParse(ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out JpegArithmeticDecodingTable? arithmeticTable, out int bytesConsumed)
    {
        bytesConsumed = 0;
        if (buffer.IsEmpty)
        {
            arithmeticTable = null;
            return false;
        }

        byte tableClassAndIdentifier = buffer[0];
        bytesConsumed++;

        return TryParse((byte)(tableClassAndIdentifier >> 4), (byte)(tableClassAndIdentifier & 0xf), buffer.Slice(1), out arithmeticTable, ref bytesConsumed);
    }

    public static bool TryParse(byte tableClass, byte identifier, ReadOnlySequence<byte> buffer, [NotNullWhen(true)] out JpegArithmeticDecodingTable? arithmeticTable, ref int bytesConsumed)
    {
#if NO_READONLYSEQUENCE_FISTSPAN
            ReadOnlySpan<byte> data = buffer.First.Span;
#else
        ReadOnlySpan<byte> data = buffer.FirstSpan;
#endif

        return TryParse(tableClass, identifier, data, out arithmeticTable, ref bytesConsumed);
    }

    public static bool TryParse(byte tableClass, byte identifier, ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out JpegArithmeticDecodingTable? arithmeticTable, ref int bytesConsumed)
    {
        if (buffer.IsEmpty)
        {
            arithmeticTable = null;
            return false;
        }

        byte conditioningTableValue = buffer[0];
        if (tableClass == 1)
        {
            if (conditioningTableValue < 1 || conditioningTableValue > 63)
            {
                arithmeticTable = null;
                return false;
            }
        }

        arithmeticTable = new JpegArithmeticDecodingTable(tableClass, identifier);
        arithmeticTable.Configure(conditioningTableValue);
        bytesConsumed++;
        return true;
    }
}
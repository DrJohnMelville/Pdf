using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.SequenceReaders;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

public ref struct MQDecoder
{
    private uint c;
    private ushort a;
    private byte ct;
    private readonly ContextStateDict contextState;
    private byte B, B1;

    public string DebugState =>
        $"A:{a:X} C:{c:X} count:{ct} b:{B} b1:{B1}";

    public ushort CHigh
    {
        get => (ushort) (c >> 16);
        set => c = ((uint)value << 16) | (c & 0xFFFF);
}

    public MQDecoder(ref SequenceReader<byte> source, int contextStateqBits) : this()
    {
        contextState = new ContextStateDict(contextStateqBits);
        INITDEC(ref source);
    }
    public void INITDEC(ref SequenceReader<byte> source)
    {
        source.TryRead(out B);
        source.TryRead(out B1);
        c = (uint) (B ^ 0xFF) << 16;
        BYTEIN(ref source);
        c <<= 7;
        ct -= 7;
        a = 0x8000;
    }
    private void BYTEIN(ref SequenceReader<byte> source)
    {
        if (B == 0xFF)
        {
            if (IsTerminationCode())
            {
                ct = 8;
            }
            else
            {
                InnerReadByte(ref source, 0xFE00, 9, 7);
            }
            return;
        }

        InnerReadByte(ref source, 0xFF00, 8, 8);
    }

    private bool IsTerminationCode() => B1 > 0x8f;

    private void InnerReadByte(ref SequenceReader<byte> source, int max, int byteOffset, byte nextCount)
    { 
        B = B1;
        source.TryRead(out B1);
        c += (uint)(max - (B << byteOffset));
        ct = nextCount;
    }

    public int GetBit(ref SequenceReader<byte> source, ushort context)
    {
        var ret = DECODE(ref source, context);
        Debug.Assert(ret is 0 or 1);
        return ret;
    }

    private byte DECODE(ref SequenceReader<byte> source, ushort context)
    {
        ref var currentState = ref contextState.EntryForContext(context);
        ref var qeRow = ref QeComputer.Rows[currentState.I];
        a -= qeRow.Qe;
        byte ret;
        if (CHigh < a)
        {
            if ((a & 0x8000) != 0) return currentState.MPS;
            ret = MPS_EXCHANGE(ref currentState, ref qeRow);
        }
        else
        {
            CHigh -= a;
            ret = LPS_EXCHANGE(ref currentState, ref qeRow);
        }

        RENORMD(ref source);
        return ret;
    }

    private byte MPS_EXCHANGE(ref ContextEntry currentState, ref QeEntry qeRow)
    {
        if (a  >= qeRow.Qe)
        {
            currentState.I = qeRow.NMPS;
            return currentState.MPS;
        }

        return ExchangeCommon(ref currentState, qeRow);
    }

    private static byte ExchangeCommon(ref ContextEntry currentState, in QeEntry qeRow)
    {
        var ret = (byte)(1 - currentState.MPS);
        qeRow.TrySwitch(ref currentState);
        currentState.I = qeRow.NLPS;
        return ret;
    }

    private byte LPS_EXCHANGE(ref ContextEntry currentState, ref QeEntry qeRow)
    {
        if (a < qeRow.Qe)
        {
            a = qeRow.Qe;
            currentState.I = qeRow.NMPS;
            return currentState.MPS;
        }
        a = qeRow.Qe;
        return ExchangeCommon(ref currentState, qeRow);
    }

    private void RENORMD(ref SequenceReader<byte> source)
    {
        do
        {
            if (ct == 0) BYTEIN(ref source);
            a <<= 1;
            c <<= 1;
            ct--;
        } while ((a & 0x8000) == 0);
    }
}
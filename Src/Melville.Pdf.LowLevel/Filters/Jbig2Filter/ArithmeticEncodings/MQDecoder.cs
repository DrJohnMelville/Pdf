using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.SequenceReaders;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

/// <summary>
/// In this struct SCREAMINGCAPS names are the names given to operations in Annex E of the ITU T88 spec
/// </summary>

public ref struct TwoByteBuffer
{
    public byte B;
    public byte B1;

    public void Initialize(ref SequenceReader<byte> source)
    {
        source.TryRead(out B);
        source.TryRead(out B1);
    }

    public void Advance(ref SequenceReader<byte> source)
    {
        B = B1;
        source.TryRead(out B1);
    }
    
}

public ref struct MQDecoder
{
    private uint c;
    private ushort a;
    private byte ct;
    private readonly ContextStateDict contextState;
    private TwoByteBuffer input;

    public string DebugState =>
        $"A:{a:X} C:{c:X} count:{ct} b:{input.B} b1:{input.B1}";

    public ushort CHigh
    {
        get => (ushort) (c >> 16);
        set => c = ((uint)value << 16) | (c & 0xFFFF);
}

    public MQDecoder(ref SequenceReader<byte> source, int contextBytes) : this()
    {
        contextState = new ContextStateDict(contextBytes);
        input = new TwoByteBuffer();
        INITDEC(ref source);
    }
#if OLDALG
    public void INITDEC(ref SequenceReader<byte> source)
    {
        c = (uint) source.ReadBigEndianUint8() << 16;
        BYTEIN(ref source);
        c <<= 7;
        ct -= 7;
        a = 0x8000;
    }
#else
    public void INITDEC(ref SequenceReader<byte> source)
    {
        input.Initialize(ref source);
        c = (uint) (input.B ^ 0xFF) << 16;
        BYTEIN(ref source);
        c <<= 7;
        ct -= 7;
        a = 0x8000;
    }
#endif

#if OLDALG
    private void BYTEIN(ref SequenceReader<byte> source)
    {
        source.TryPeek(0, out byte b);
        if (b == 0xFF)
        {
            CheckForTerminationSequence(source);
            return;
        }
        source.Advance(1);
        c += (uint) b << 8;
        ct = 8;
    }
    private void CheckForTerminationSequence(SequenceReader<byte> source)
    {
        const int b = 0xFF;
        source.TryPeek(1, out var b1);
        if (b1 > 0x8f)
        {
            c += 0xFF00;
            ct = 8;
        }
        else
        {
            source.Advance(1);
            c += (uint)b << 9;
            ct = 7;
        }
    }
#else
    private void BYTEIN(ref SequenceReader<byte> source)
    {
        if (input.B == 0xFF)
        {
            if (input.B1 > 0x8f)
            {
                ct = 8;
            }
            else
            {
                input.Advance(ref source);
                c += (uint)(0xfe00 - (input.B << 9));
                ct = 7;
            }
            return;
        }
        input.Advance(ref source);
        c += (uint)(0xFF00 - (input.B << 8));
        ct = 8;
    }
#endif

    public int GetBit(ref SequenceReader<byte> source, ushort context)
    {
        var ret = DECODE(ref source, context);
        Debug.Assert(ret is 0 or 1);
        return ret;
    }

    #if OLDALG
    private byte DECODE(ref SequenceReader<byte> source, ushort context)
    {
        ref var currentState = ref contextState.EntryForContext(context);
        ref var qeRow = ref QeComputer.Rows[currentState.I];
        a -= qeRow.Qe;
        byte ret;
        if (CHigh < qeRow.Qe)
        {
            CHigh -= qeRow.Qe;
            if ((a & 0x8000) != 0) return currentState.MPS;
            ret = MPS_EXCHANGE(ref currentState, ref qeRow);
        }
        else
        {
            ret = LPS_EXCHANGE(ref currentState, ref qeRow);
        }

        RENORMD(ref source);
        return ret;
    }
#else
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
#endif

    private byte MPS_EXCHANGE(ref ContextEntry currentState, ref QeEntry qeRow)
    {
        if (a  >= qeRow.Qe)
        {
            currentState.I = qeRow.NMPS;
            return currentState.MPS;
        }

        // note duplication in LPS_Exchange
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
        // note duplication in MPSEXCHANGE
        var ret = (byte) (1 - currentState.MPS);
        qeRow.TrySwitch(ref currentState);
        currentState.I = qeRow.NLPS;
        return ret;
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
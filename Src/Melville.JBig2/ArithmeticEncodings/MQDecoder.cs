using System.Buffers;
using System.Diagnostics;

namespace Melville.JBig2.ArithmeticEncodings;

[DebuggerDisplay("A:{a} C:{c} count:{ct} b:{B} b1:{B1}")]
public class MQDecoder
{
    private const int UninitializedASentinelValue = 1;
    private uint c;
    private uint a = UninitializedASentinelValue;
    private byte ct;
    private byte B, B1;

    public int GetBit(ref SequenceReader<byte> source, ref ContextEntry context)
    {
        EnsureIsInitialized(ref source);
        var ret = DECODE(ref source, ref context);
        Debug.Assert(ret is 0 or 1);
        return ret;
    }

    private void EnsureIsInitialized(ref SequenceReader<byte> source)
    {
        if (IsUninitialized()) INITDEC(ref source);
    }

    //a is shifted 16 bits left so it can interact with CHigh.  This means the lower 16 bytes of A should always be
    // 0.  We use bit 1 as a sentinel value to detect then the decoder is not initialized.
    private bool IsUninitialized() => a == UninitializedASentinelValue;

    public void INITDEC(ref SequenceReader<byte> source) // uses software convention pg 119
    {
        source.TryRead(out B);
        source.TryRead(out B1);
        c = (uint) (B ^ 0xFF) << 16;
        BYTEIN(ref source);
        c <<= 7;
        ct -= 7;
        a = 0x8000_0000;
    }
    
    private void BYTEIN(ref SequenceReader<byte> source) // software convention page 120
    {
        switch (B, B1)
        {
            case (255, >0x8f):
                ct = 8;
                break;
            case (255,_):
                InnerReadByte(ref source, 0xFE00, 9, 7);
                break;
            default:
                InnerReadByte(ref source, 0xFF00, 8, 8);
                break;                
        }
    }
    
    private void InnerReadByte(ref SequenceReader<byte> source, int max, int byteOffset, byte nextCount)
    { 
        B = B1;
        source.TryRead(out B1);
        c += (uint)(max - (B << byteOffset));
        ct = nextCount;
    }
    
    // software convention page 120
    private byte DECODE(ref SequenceReader<byte> source, ref ContextEntry currentState)
    {
        ref var qeRow = ref QeComputer.Rows[currentState.I];
        a -= qeRow.Qe;
        Debug.Assert((a & 0xFFFF) == 0 );
        byte ret;
        if (c < a)
        {
            if ((a & 0x8000_0000) != 0) return currentState.MPS;
            ret = MPS_EXCHANGE(ref currentState, ref qeRow);
        }
        else
        {
            c -= a;
            ret = LPS_EXCHANGE(ref currentState, ref qeRow);
        }

        RENORMD(ref source);
        return ret;
    }

    private byte MPS_EXCHANGE(ref ContextEntry currentState, ref QeEntry qeRow)
    {
        if (a < qeRow.Qe) 
            return ExchangeCommon(ref currentState, qeRow);
        
        currentState.I = qeRow.NMPS;
        return currentState.MPS;

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

    private static byte ExchangeCommon(ref ContextEntry currentState, in QeEntry qeRow)
    {
        var ret = (byte)(1 - currentState.MPS);
        qeRow.TrySwitch(ref currentState);
        currentState.I = qeRow.NLPS;
        return ret;
    }


    private void RENORMD(ref SequenceReader<byte> source)
    {
        Debug.Assert((a & 0xFFFF) == 0 );
        do
        {
            if (ct == 0) BYTEIN(ref source);
            a <<= 1;
            c <<= 1;
            ct--;
        } while ((a & 0x80000000) == 0);
        Debug.Assert((a & 0xFFFF) == 0 );

    }
}
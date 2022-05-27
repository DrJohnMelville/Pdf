using System;
using System.Buffers;
using System.Diagnostics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public enum CcittCodeOperation : byte
{
    Pass = 0,
    HorizontalBlack = 1,
    HorizontalWhite = 2,
    Vertical = 3,
    MakeUp = 4,
    SwitchToHorizontalMode = 5,
    EndOfLine = 6,
    NoCode = 7,
}

public interface ICodeDictionay
{
    bool TryReadCode(
        in (int BitLength, int SourceBits) input, bool isWhiteRun, out CcittCode code);
    bool IsAtValidEndOfLine { get; }
}

public partial class CcittCodeReader
{
    private readonly BitReader reader = new();
    private int currentWord = 0;
    private int currentWordLength = 0;
    private readonly ICodeDictionay dict;

    public CcittCodeReader(ICodeDictionay dict)
    {
        this.dict = dict;
    }

    public bool TryReadEndOfFileCode(ref SequenceReader<byte> source, out bool isEndOfFile)
    {
        var privateSource = source;
        var privateReader = reader;
        isEndOfFile = false;
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 11; j++)
            {
                if (!privateReader.TryRead(1, ref privateSource, out var zeroBit))
                    return false; // not enough data
                if (zeroBit != 0) return true; // found wrong code;
            }

            if (!privateReader.TryRead(1, ref privateSource, out var oneBit)) return false;
            if (oneBit != 1) return true;
        }
        isEndOfFile = true;
        source = privateSource;
        return true;
    }
    

    public bool TryReadCode(ref SequenceReader<byte> source, bool isWhiteRun, out CcittCode code)
    {
        while (true)
        {
            if (!ReadSingleCodeWord(ref source, isWhiteRun, out code)) return false;
             if (code.Operation != CcittCodeOperation.NoCode)
            {
                Debug.Assert(code.Operation != CcittCodeOperation.MakeUp);
                Debug.Assert(code.Operation != CcittCodeOperation.SwitchToHorizontalMode);
                return true;
            }
        }
    }

    private bool ReadSingleCodeWord(ref SequenceReader<byte> source, bool isWhiteRun, out CcittCode code)
    {
        do
        {
            if (!reader.TryRead(1, ref source, out var bit)) 
                return NotEnoughBitsForCompleteCode(out code);
            AddBitToCurrentWord(bit);
        } while (!LookupCode(isWhiteRun, out code));
        ResetCurrentWord();
        return true;
    }

    private static bool NotEnoughBitsForCompleteCode(out CcittCode code)
    {
        code = new CcittCode(CcittCodeOperation.NoCode, 0);
        return false;
    }

    private bool LookupCode(bool isWhiteRun, out CcittCode code) => 
        dict.TryReadCode((currentWordLength, currentWord), isWhiteRun, out code);


    private void ResetCurrentWord()
    {
        currentWord = 0;
        currentWordLength = 0;
    }
    
    private void AddBitToCurrentWord(int bit)
    {
        Debug.Assert(bit is 0 or 1);
        currentWord <<= 1;
        currentWord |= bit;
        currentWordLength++;
    }
    public void DiscardPartialByte()
    {
        Debug.Assert(currentWordLength == 0); // we should not be discarding in the middle of a code
        reader.DiscardPartialByte();
    }
}
using System.Buffers;
using System.Collections.Generic;
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
    SwitchToHorizontalMode = 5
}

public record struct CcittCode(CcittCodeOperation Operation, ushort Length);

public partial class CcittCodeReader
{
    private Dictionary<(int, int), CcittCode> operationCodeBook = new()
    {
        //Pass Mode
        {(4, 0b0001), new CcittCode(CcittCodeOperation.Pass, 0)},
        // verticalCodes
        {(1,0b1), new CcittCode(CcittCodeOperation.Vertical, 3)},
        {(3,0b011), new CcittCode(CcittCodeOperation.Vertical, 4)},
        {(6,0b000011), new CcittCode(CcittCodeOperation.Vertical, 5)},
        {(7,0b0000011), new CcittCode(CcittCodeOperation.Vertical, 6)},
        {(3,0b010), new CcittCode(CcittCodeOperation.Vertical, 2)},
        {(6,0b000010), new CcittCode(CcittCodeOperation.Vertical, 1)},
        {(7,0b0000010), new CcittCode(CcittCodeOperation.Vertical, 0)},
        //Transition to horizontalMode
        {(3, 0b001), new CcittCode(CcittCodeOperation.SwitchToHorizontalMode, 0)}
    };

[MacroItem("HorizontalWhite", 0, 8, "00110101")]
[MacroItem("HorizontalWhite", 1, 6, "000111")]
[MacroItem("HorizontalWhite", 2, 4, "0111")]
[MacroItem("HorizontalWhite", 3, 4, "1000")]
[MacroItem("HorizontalWhite", 4, 4, "1011")]
[MacroItem("HorizontalWhite", 5, 4, "1100")]
[MacroItem("HorizontalWhite", 6, 4, "1110")]
[MacroItem("HorizontalWhite", 7, 4, "1111")]
[MacroItem("HorizontalWhite", 6, 5, "10011")]
[MacroItem("HorizontalWhite", 9, 5, "10100")]
[MacroItem("HorizontalWhite", 10, 5, "00111")]
[MacroItem("HorizontalWhite", 11, 5, "01000")]
[MacroItem("HorizontalWhite", 12, 6, "001000")]
[MacroItem("HorizontalWhite", 13, 6, "000011")]
[MacroItem("HorizontalWhite", 14, 6, "110100")]
[MacroItem("HorizontalWhite", 15, 6, "110101")]
[MacroItem("HorizontalWhite", 16, 6, "101010")]
[MacroItem("HorizontalWhite", 17, 6, "101011")]
[MacroItem("HorizontalWhite", 18, 7, "0100111")]
[MacroItem("HorizontalWhite", 19, 7, "0001100")]
[MacroItem("HorizontalWhite", 20, 7, "0001000")]
[MacroItem("HorizontalWhite", 21, 7, "0010111")]
[MacroItem("HorizontalWhite", 22, 7, "0000011")]
[MacroItem("HorizontalWhite", 23, 7, "0000100")]
[MacroItem("HorizontalWhite", 24, 7, "0101000")]
[MacroItem("HorizontalWhite", 25, 7, "0101011")]
[MacroItem("HorizontalWhite", 26, 7, "0010011")]
[MacroItem("HorizontalWhite", 27, 7, "0100100")]
[MacroItem("HorizontalWhite", 28, 7, "0011000")]
[MacroItem("HorizontalWhite", 29, 8, "00000010")]
[MacroItem("HorizontalWhite", 30, 8, "00000011")]
[MacroItem("HorizontalWhite", 31, 8, "00011010")]
[MacroItem("HorizontalWhite", 32, 8, "00011011")]
[MacroItem("HorizontalWhite", 33, 8, "00010010")]
[MacroItem("HorizontalWhite", 34, 8, "00010011")]
[MacroItem("HorizontalWhite", 35, 8, "00010100")]
[MacroItem("HorizontalWhite", 36, 8, "00010101")]
[MacroItem("HorizontalWhite", 37, 8, "00010110")]
[MacroItem("HorizontalWhite", 38, 8, "00010111")]
[MacroItem("HorizontalWhite", 39, 8, "00101000")]
[MacroItem("HorizontalWhite", 40, 8, "00101001")]
[MacroItem("HorizontalWhite", 41, 8, "00101010")]
[MacroItem("HorizontalWhite", 42, 8, "00101011")]
[MacroItem("HorizontalWhite", 43, 8, "00101100")]
[MacroItem("HorizontalWhite", 44, 8, "00101101")]
[MacroItem("HorizontalWhite", 45, 8, "00000100")]
[MacroItem("HorizontalWhite", 46, 8, "00000101")]
[MacroItem("HorizontalWhite", 47, 8, "00001010")]
[MacroItem("HorizontalWhite", 48, 8, "00001011")]
[MacroItem("HorizontalWhite", 49, 8, "01010010")]
[MacroItem("HorizontalWhite", 50, 8, "01010011")]
[MacroItem("HorizontalWhite", 51, 8, "01010100")]
[MacroItem("HorizontalWhite", 52, 8, "01010101")]
[MacroItem("HorizontalWhite", 53, 8, "00100100")]
[MacroItem("HorizontalWhite", 54, 8, "00100101")]
[MacroItem("HorizontalWhite", 55, 8, "01011000")]
[MacroItem("HorizontalWhite", 56, 8, "01011001")]
[MacroItem("HorizontalWhite", 57, 8, "01011010")]
[MacroItem("HorizontalWhite", 58, 8, "01011011")]
[MacroItem("HorizontalWhite", 59, 8, "01001010")]
[MacroItem("HorizontalWhite", 60, 8, "01001011")]
[MacroItem("HorizontalWhite", 61, 8, "00110010")]
[MacroItem("HorizontalWhite", 62, 8, "00110011")]
[MacroItem("HorizontalWhite", 63, 8, "00110100")]
[MacroItem("MakeUp", 64, 5, "11011")]
[MacroItem("MakeUp", 128, 5, "10010")]
[MacroItem("MakeUp", 192, 6, "010111")]
[MacroItem("MakeUp", 256, 7, "0110111")]
[MacroItem("MakeUp", 320, 8, "00110110")]
[MacroItem("MakeUp", 384, 8, "00110111")]
[MacroItem("MakeUp", 448, 8, "01100100")]
[MacroItem("MakeUp", 512, 8, "01100101")]
[MacroItem("MakeUp", 576, 8, "01101000")]
[MacroItem("MakeUp", 640, 8, "01100111")]
[MacroItem("MakeUp", 704, 9, "011001100")]
[MacroItem("MakeUp", 768, 9, "011001101")]
[MacroItem("MakeUp", 832, 9, "011010010")]
[MacroItem("MakeUp", 896, 9, "011010011")]
[MacroItem("MakeUp", 960, 9, "011010100")]
[MacroItem("MakeUp", 1024, 9, "011010101")]
[MacroItem("MakeUp", 1088, 9, "011010110")]
[MacroItem("MakeUp", 1152, 9, "011010111")]
[MacroItem("MakeUp", 1216, 9, "011011000")]
[MacroItem("MakeUp", 1280, 9, "011011001")]
[MacroItem("MakeUp", 1344, 9, "011011010")]
[MacroItem("MakeUp", 1408, 9, "011011011")]
[MacroItem("MakeUp", 1472, 9, "010011000")]
[MacroItem("MakeUp", 1536, 9, "010011001")]
[MacroItem("MakeUp", 1600, 9, "010011010")]
[MacroItem("MakeUp", 1664, 6, "011000")]
[MacroItem("MakeUp", 1728, 9, "010011011")]
[MacroItem("MakeUp", 1792, 11, "00000001000")]
[MacroItem("MakeUp", 1856, 11, "00000001100")]
[MacroItem("MakeUp", 1920, 11, "00000001101")]
[MacroItem("MakeUp", 1984, 12, "000000010010")]
[MacroItem("MakeUp", 2048, 12, "000000010011")]
[MacroItem("MakeUp", 2112, 12, "000000010100")]
[MacroItem("MakeUp", 2176, 12, "000000010101")]
[MacroItem("MakeUp", 2240, 12, "000000010110")]
[MacroItem("MakeUp", 2304, 12, "000000010111")]
[MacroItem("MakeUp", 2368, 12, "000000011100")]
[MacroItem("MakeUp", 2432, 12, "000000011101")]
[MacroItem("MakeUp", 2496, 12, "000000011110")]
[MacroItem("MakeUp", 2560, 12, "000000011111")]
[MacroCode(  "        {(~2~,0b~3~), new CcittCode(CcittCodeOperation.~0~, ~1~)},\r\n",
        Prefix  ="    private Dictionary<(int, int), CcittCode> whiteCodeBook = new() {",
        Postfix = "    };")]
    private readonly CcittCode initiateHorizontal = new(CcittCodeOperation.SwitchToHorizontalMode, 0);
[MacroItem("HorizontalBlack", 0, 10, "0000110111")]
[MacroItem("HorizontalBlack", 1, 3, "010")]
[MacroItem("HorizontalBlack", 2, 2, "11")]
[MacroItem("HorizontalBlack", 3, 2, "10")]
[MacroItem("HorizontalBlack", 4, 3, "011")]
[MacroItem("HorizontalBlack", 5, 4, "0011")]
[MacroItem("HorizontalBlack", 6, 4, "0010")]
[MacroItem("HorizontalBlack", 7, 5, "00011")]
[MacroItem("HorizontalBlack", 6, 6, "000101")]
[MacroItem("HorizontalBlack", 9, 6, "000100")]
[MacroItem("HorizontalBlack", 10, 7, "0000100")]
[MacroItem("HorizontalBlack", 11, 7, "0000101")]
[MacroItem("HorizontalBlack", 12, 7, "0000111")]
[MacroItem("HorizontalBlack", 13, 8, "00000100")]
[MacroItem("HorizontalBlack", 14, 8, "00000111")]
[MacroItem("HorizontalBlack", 15, 9, "000011000")]
[MacroItem("HorizontalBlack", 16, 10, "0000010111")]
[MacroItem("HorizontalBlack", 17, 10, "0000011000")]
[MacroItem("HorizontalBlack", 18, 10, "0000001000")]
[MacroItem("HorizontalBlack", 19, 11, "00001100111")]
[MacroItem("HorizontalBlack", 20, 11, "00001101000")]
[MacroItem("HorizontalBlack", 21, 11, "00001101100")]
[MacroItem("HorizontalBlack", 22, 11, "00000110111")]
[MacroItem("HorizontalBlack", 23, 11, "00000101000")]
[MacroItem("HorizontalBlack", 24, 11, "00000010111")]
[MacroItem("HorizontalBlack", 25, 11, "00000011000")]
[MacroItem("HorizontalBlack", 26, 12, "000011001010")]
[MacroItem("HorizontalBlack", 27, 12, "000011001011")]
[MacroItem("HorizontalBlack", 28, 12, "000011001100")]
[MacroItem("HorizontalBlack", 29, 12, "000011001101")]
[MacroItem("HorizontalBlack", 30, 12, "000001101000")]
[MacroItem("HorizontalBlack", 31, 12, "000001101001")]
[MacroItem("HorizontalBlack", 32, 12, "000001101010")]
[MacroItem("HorizontalBlack", 33, 12, "000001101011")]
[MacroItem("HorizontalBlack", 34, 12, "000011010010")]
[MacroItem("HorizontalBlack", 35, 12, "000011010011")]
[MacroItem("HorizontalBlack", 36, 12, "000011010100")]
[MacroItem("HorizontalBlack", 37, 12, "000011010101")]
[MacroItem("HorizontalBlack", 38, 12, "000011010110")]
[MacroItem("HorizontalBlack", 39, 12, "000011010111")]
[MacroItem("HorizontalBlack", 40, 12, "000001101100")]
[MacroItem("HorizontalBlack", 41, 12, "000001101101")]
[MacroItem("HorizontalBlack", 42, 12, "000011011010")]
[MacroItem("HorizontalBlack", 43, 12, "000011011011")]
[MacroItem("HorizontalBlack", 44, 12, "000001010100")]
[MacroItem("HorizontalBlack", 45, 12, "000001010101")]
[MacroItem("HorizontalBlack", 46, 12, "000001010110")]
[MacroItem("HorizontalBlack", 47, 12, "000001010111")]
[MacroItem("HorizontalBlack", 48, 12, "000001100100")]
[MacroItem("HorizontalBlack", 49, 12, "000001100101")]
[MacroItem("HorizontalBlack", 50, 12, "000001010010")]
[MacroItem("HorizontalBlack", 51, 12, "000001010011")]
[MacroItem("HorizontalBlack", 52, 12, "000000100100")]
[MacroItem("HorizontalBlack", 53, 12, "000000110111")]
[MacroItem("HorizontalBlack", 54, 12, "000000111000")]
[MacroItem("HorizontalBlack", 55, 12, "000000100111")]
[MacroItem("HorizontalBlack", 56, 12, "000000101000")]
[MacroItem("HorizontalBlack", 57, 12, "000001011000")]
[MacroItem("HorizontalBlack", 58, 12, "000001011001")]
[MacroItem("HorizontalBlack", 59, 12, "000000101011")]
[MacroItem("HorizontalBlack", 60, 12, "000000101100")]
[MacroItem("HorizontalBlack", 61, 12, "000001011010")]
[MacroItem("HorizontalBlack", 62, 12, "000001100110")]
[MacroItem("HorizontalBlack", 63, 12, "000001100111")]
[MacroItem("MakeUp", 64, 10, "0000001111")]
[MacroItem("MakeUp", 128, 12, "000011001000")]
[MacroItem("MakeUp", 192, 12, "000011001001")]
[MacroItem("MakeUp", 256, 12, "000001011011")]
[MacroItem("MakeUp", 320, 12, "000000110011")]
[MacroItem("MakeUp", 384, 12, "000000110100")]
[MacroItem("MakeUp", 448, 12, "000000110101")]
[MacroItem("MakeUp", 512, 13, "0000001101100")]
[MacroItem("MakeUp", 576, 13, "0000001101101")]
[MacroItem("MakeUp", 640, 13, "0000001001010")]
[MacroItem("MakeUp", 704, 13, "0000001001011")]
[MacroItem("MakeUp", 768, 13, "0000001001100")]
[MacroItem("MakeUp", 832, 13, "0000001001101")]
[MacroItem("MakeUp", 896, 13, "0000001110010")]
[MacroItem("MakeUp", 960, 13, "0000001110011")]
[MacroItem("MakeUp", 1024, 13, "0000001110100")]
[MacroItem("MakeUp", 1088, 13, "0000001110101")]
[MacroItem("MakeUp", 1152, 13, "0000001110110")]
[MacroItem("MakeUp", 1216, 13, "0000001110111")]
[MacroItem("MakeUp", 1280, 13, "0000001010010")]
[MacroItem("MakeUp", 1344, 13, "0000001010011")]
[MacroItem("MakeUp", 1408, 13, "0000001010100")]
[MacroItem("MakeUp", 1472, 13, "0000001010101")]
[MacroItem("MakeUp", 1536, 13, "0000001011010")]
[MacroItem("MakeUp", 1600, 13, "0000001011011")]
[MacroItem("MakeUp", 1664, 13, "0000001100100")]
[MacroItem("MakeUp", 1728, 13, "0000001100101")]
[MacroItem("MakeUp", 1792, 11, "00000001000")]
[MacroItem("MakeUp", 1856, 11, "00000001100")]
[MacroItem("MakeUp", 1920, 11, "00000001101")]
[MacroItem("MakeUp", 1984, 12, "000000010010")]
[MacroItem("MakeUp", 2048, 12, "000000010011")]
[MacroItem("MakeUp", 2112, 12, "000000010100")]
[MacroItem("MakeUp", 2176, 12, "000000010101")]
[MacroItem("MakeUp", 2240, 12, "000000010110")]
[MacroItem("MakeUp", 2304, 12, "000000010111")]
[MacroItem("MakeUp", 2368, 12, "000000011100")]
[MacroItem("MakeUp", 2432, 12, "000000011101")]
[MacroItem("MakeUp", 2496, 12, "000000011110")]
[MacroItem("MakeUp", 2560, 12, "000000011111")]
[MacroCode(  "        {(~2~,0b~3~), new CcittCode(CcittCodeOperation.~0~, ~1~)},\r\n",
        Prefix  ="    private Dictionary<(int, int), CcittCode> blackCodeBook = new() {",
        Postfix = "    };")]
    private readonly BitReader reader = new();
    private int currentWord = 0;
    private int currentWordLength = 0;
    private int expectedHorizontalRuns = 0;
    

    public bool TryReadCode(ref SequenceReader<byte> source, bool isWhiteRun, out CcittCode code)
    {
        while (true)
        {
            do
            {
                if (!reader.TryRead(1, ref source, out var bit))
                {
                    code = initiateHorizontal;
                    return false;
                }
                AddBitToCurrentWord(bit);
            } while (!LookupCode(isWhiteRun, out code));
            ResetCurrentWord();
            if (GetTerminalCode(ref code)) return true;
        }
    }

    private bool LookupCode(bool isWhiteRun, out CcittCode code) => 
        ActiveCodeBook(isWhiteRun).TryGetValue((currentWordLength, currentWord), out code);

   private Dictionary<(int, int), CcittCode> ActiveCodeBook(bool isWhiteRun) => 
       (expectedHorizontalRuns, isWhiteRun) switch
       {
           (<1,_) => operationCodeBook,
           (_, true) => whiteCodeBook,
           (_,false) => blackCodeBook,
       };

    private void ResetCurrentWord()
    {
        currentWord = 0;
        currentWordLength = 0;
    }

    private bool GetTerminalCode(ref CcittCode code) => (code.Operation) switch
    {
        CcittCodeOperation.HorizontalWhite or CcittCodeOperation.HorizontalBlack => FixupLength(ref code),
        CcittCodeOperation.MakeUp => HandleMakeup(code),
        CcittCodeOperation.SwitchToHorizontalMode => StartHorizontalMode(),
        _ => true
    };

    private bool StartHorizontalMode()
    {
        expectedHorizontalRuns = 2;
        return false;
    }

    private bool HandleMakeup(CcittCode code)
    {
        makeupLength += code.Length;
        return false;
    }

    private ushort makeupLength;
    private bool FixupLength(ref CcittCode code)
    {
        expectedHorizontalRuns--;
        code = code with { Length = (ushort)(code.Length + makeupLength) };
        makeupLength = 0;
        return true;
    }

    private void AddBitToCurrentWord(int bit)
    {
        Debug.Assert(bit is 0 or 1);
        currentWord <<= 1;
        currentWord |= bit;
        currentWordLength++;
    }
}
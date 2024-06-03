using System.Buffers;
using System.Runtime.InteropServices;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.INPC;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

[StructLayout(LayoutKind.Explicit)]
internal readonly struct DictValue
{
    [FieldOffset(0)] private readonly int intValue;
    [FieldOffset(0)] private readonly float floatValue;
    public DictValue(int value)  => intValue = value;
    public DictValue(float value)  => floatValue = value;
    public int IntValue => intValue;
    public float FloatValue => floatValue;
}

internal ref partial struct DictScanner
{
    [FromConstructor] private SequenceReader<byte> source;
    [FromConstructor] private readonly int operatorDesired;
    [FromConstructor] private readonly Span<DictValue> operands;
    private int operandPosition = 0;
    public bool TryFindEntry()
    {
        while (source.TryRead(out var b))
        {
            if (TryReadToken(b)) return true;
        }

        return false;
    }

    private bool TryReadToken(byte value)
    {
        switch (value)
        {
            // handle prefix operator
            case < 22: return value == operatorDesired;
            case 28: 
                ReadRawTwoByteInteger();
                break;
            case 29:
                ReadFiveByteInteger();
                break;
            case 30:
                ReadSingle();
                break;
            case < 247: 
                PushOperand(new DictValue(value - 139)); 
                break;
            case < 251:
                Parse2ByteInteger(value);
                break;
            case < 255:
                ParseNegativeTwoByteInteger(value);
                break;
            default:
                throw new InvalidDataException("Invalid string in Dictionary");
        }

        return false;
    }

    private void ReadSingle()
    {
        Span<char> stringValue = stackalloc char[25];
        int pos = 0;
        while (source.TryRead(out var b) &&
               AddNibble(b >> 4, stringValue, ref pos) &&
               AddNibble(b & 0xF, stringValue, ref pos)
              ) ; // do nothing in loop
        PushOperand(new DictValue(float.Parse(stringValue[..pos])));
        
    }

    private bool AddNibble(int i, scoped Span<char> stringValue, ref int pos)
    {
        if (i == 15) return false;
        stringValue[pos++] = ReadDigit(i);
        if (i is 0XC) stringValue[pos++] = '-';
        return true;
    }

    private static char ReadDigit(int i) =>
      i switch
        {
            < 10 => (char)('0' + i),
            0xA => '.',
            0xB => 'E',
            0xC => 'E',
            0xE => '-',
            _ => throw new InvalidDataException("Invalid nibble in real number")
        };

    private void ReadFiveByteInteger()
    {
        if (!(source.TryRead(out var b1) && source.TryRead(out var b2)
                                         && source.TryRead(out var b3) && source.TryRead(out var b4))) 
            return;
        PushOperand(new DictValue((b1 << 24) | (b2 << 16) | (b3 << 8) + b4));
    }

    private bool IsOperator(byte b) => b < 22;
    private const byte PrefixOperator = 12;

    private void ReadRawTwoByteInteger()
    {
        if (!(source.TryRead(out var b1) && source.TryRead(out var b2))) return;
        PushOperand(new DictValue((short)((b1 << 8) + b2)));
    }

    private void ParseNegativeTwoByteInteger(byte value)
    {
        if (!source.TryRead(out var b1))
            return;
        PushOperand(new DictValue(-(value - 251) * 256 - b1 - 108));
    }

    private void Parse2ByteInteger(byte value)
    {
        if (!source.TryRead(out var second)) return;
        PushOperand(new DictValue((value - 247) * 256 + second + 108));
    }

    private void PushOperand(DictValue value)
    {
        operands[operandPosition] = value;
        operandPosition++;
        operandPosition %= operands.Length;
    }
}
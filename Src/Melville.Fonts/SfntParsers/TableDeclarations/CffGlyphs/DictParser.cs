using System.Buffers;
using Melville.INPC;
using Melville.Parsing.ParserMapping;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal ref partial struct DictParser<T> where T: IDictionaryDefinition
{
    [FromConstructor] private SequenceReader<byte> source;
    [FromConstructor] private readonly ParseMapBookmark? bookmark;
    [FromConstructor] private readonly Span<DictValue> operands;
    public int OperandPosition { get; private set; } = 0;
    public ReadOnlySequence<byte> UnreadSequence => source.UnreadSequence;

    public ReadOnlySpan<DictValue> Operands => operands[..OperandPosition];

    public bool TryFindEntry(int operatorDesired)
    {
        while (true)
        {
            var op = ReadNextInstruction();
            if (op == operatorDesired) return true;
            if (op == 0xFF) return false;
        }
    }

    public int ReadNextInstruction(int startingStackSize = 0)
    {
        OperandPosition = startingStackSize;
        while (source.TryRead(out var value))
        {
            switch (T.ClassifyEntry(value))
            {
                // handle prefix operator
                case DictParserOperations.TwoByteInstruction: 
                    return LogInstruction(ReadTwoByteInstruction());
                case DictParserOperations.OneByteInstruction: return LogInstruction(value);
                case DictParserOperations.RawTwoByteInteger:
                    ReadRawTwoByteInteger();
                    break;
                case DictParserOperations.FiveByteInteger:
                    ReadFiveByteInteger();
                    break;
                case DictParserOperations.SingleFloat:
                    ReadSingle();
                    break;
                case DictParserOperations.OneByteInteger:
                    PushOperand(new DictValue(value - 139));
                    break;
                case DictParserOperations.TwoBytePositiveInteger:
                    Parse2ByteInteger(value);
                    break;
                case DictParserOperations.TwoByteNegativeInteger:
                    ParseNegativeTwoByteInteger(value);
                    break;
                case DictParserOperations.FiveByteFixed:
                    ReadFiveByteFixed();
                    break;
                default:
                    throw new InvalidDataException("Unknown Dict Operator");
            }
        }

        return 0xFF;
    }

    private int LogInstruction(int instruction)
    {
        bookmark.LogParsePosition($"{DictionaryOperatorNamer.Title(instruction)}", (int)source.Consumed);
        return instruction;
    }

    private int ReadTwoByteInstruction() => 
        source.TryRead(out var b1) ? (12<<8) | b1 :0xFF;

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
        if (TryRead5ByteInt(out var intValue))
             PushOperand(new DictValue(intValue));
    }

    private bool TryRead5ByteInt(out int intValue)
    {
        intValue = 0;
        for (int i = 0; i < 4; i++)
        {
            if (!source.TryRead(out var b)) return false;
            intValue <<= 8;
            intValue |= b;
        }
        return true;
    }

    private const float fixedFactor = 1.0f / 0x10000;
    private void ReadFiveByteFixed()
    {
        if (TryRead5ByteInt(out var intValue)) 
            PushOperand(new DictValue(intValue*fixedFactor));
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
        PushOperand(new DictValue((251-value) * 256 - b1 - 108));
    }

    private void Parse2ByteInteger(byte value)
    {
        if (!source.TryRead(out var second)) return;
        PushOperand(new DictValue((value - 247) * 256 + second + 108));
    }

    private void PushOperand(DictValue value)
    {
        operands[OperandPosition] = value;
        bookmark.LogParsePosition($"Operand {OperandPosition}: {value}", (int)source.Consumed);
        OperandPosition++;
        OperandPosition %= operands.Length;
    }
}
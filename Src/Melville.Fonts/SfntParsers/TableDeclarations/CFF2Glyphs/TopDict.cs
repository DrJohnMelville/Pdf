using System.Buffers;
using System.Numerics;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;

internal readonly struct TopDict
{
    public readonly int CharStringOffset;
    public readonly int FontDictIndexOffset;
    public readonly int FontDictSelectOffset;
    public readonly int VariationStoreOffset;
    public readonly Matrix3x2 FontMatrix;

#warning this needs to take a span, not a sequence
    public TopDict(ReadOnlySequence<byte> source)
    {
        FontMatrix = Matrix3x2.CreateScale(0.001f);

#warning memory inefficient remove ToArray once I get a memory
        var reader = new SequenceReader<byte>(source);
        var parser = new DictParser<CffDictionaryDefinition>(
            source.ToArray(), null, stackalloc DictValue[6]);

        while (parser.ReadNextInstruction(0) is var instr and not 255)
        {
            switch (instr)
            {
                case 0x11:
                    CharStringOffset = parser.Operands[0].IntValue;
                    break;
                case 0x18:
                    VariationStoreOffset = parser.Operands[0].IntValue;
                    break;
                case 0x0c24: 
                    FontDictIndexOffset = parser.Operands[0].IntValue;
                    break;
                case 0x0c25:
                    FontDictSelectOffset = parser.Operands[0].IntValue;
                    break;
                case 0x0c07:
                    FontMatrix = new Matrix3x2(
                        parser.Operands[0].FloatValue,
                        parser.Operands[1].FloatValue,
                        parser.Operands[2].FloatValue,
                        parser.Operands[3].FloatValue,
                        parser.Operands[4].FloatValue,
                        parser.Operands[5].FloatValue);
                    break;
            }
        }
    }
}
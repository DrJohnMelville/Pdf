namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal enum DictParserOperations
{
    OneByteInstruction = 0,
    TwoByteInstruction = 1,
    RawTwoByteInteger = 2,
    FiveByteInteger = 3,
    SingleFloat = 4,
    OneByteInteger = 5,
    TwoBytePositiveInteger = 6,
    TwoByteNegativeInteger = 7
}

internal interface IDictionaryDefinition
{
    public static abstract DictParserOperations ClassifyEntry(byte data);
}

internal struct CffDictionaryDefinition: IDictionaryDefinition
{
    public static DictParserOperations ClassifyEntry(byte data) =>
        data switch
        {
            12 => DictParserOperations.TwoByteInstruction,
            <22 => DictParserOperations.OneByteInstruction,
            28 => DictParserOperations.RawTwoByteInteger,
            29 => DictParserOperations.FiveByteInteger,
            30 => DictParserOperations.SingleFloat,
            <247 => DictParserOperations.OneByteInteger,
            <251 => DictParserOperations.TwoBytePositiveInteger,
            <255 => DictParserOperations.TwoByteNegativeInteger,
            _ => throw new InvalidDataException("Invalid byte in CharString")
        };

}

internal struct CharString2Definition: IDictionaryDefinition
{
    public static DictParserOperations ClassifyEntry(byte data) =>
        data switch
        {
            12 => DictParserOperations.TwoByteInstruction,
            28 => DictParserOperations.RawTwoByteInteger,
            <32 => DictParserOperations.OneByteInstruction,
            <247 => DictParserOperations.OneByteInteger,
            <251 => DictParserOperations.TwoBytePositiveInteger,
            <255 => DictParserOperations.TwoByteNegativeInteger,
            255 => DictParserOperations.FiveByteInteger,
        };

}
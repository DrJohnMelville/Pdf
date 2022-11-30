namespace Melville.Pdf.LowLevel.Model.Conventions;

public enum CharacterClass
{
    Regular = 0,
    White = 1,
    Delimiter = 2
}

public static class CharClassifier
{
    public static CharacterClass Classify(byte input)
    {
        return input switch
        {
            0 or 0x9 or 0xA or 0xC or 0xD or 0x20
                => CharacterClass.White,
            (byte)'(' or (byte)')' or (byte)'<' or (byte)'>' or (byte)'[' or (byte)']' or
                (byte)'{' or (byte)'}' or (byte)'/' or (byte)'%' 
                => CharacterClass.Delimiter,
            _ => CharacterClass.Regular
        };
    }

    public static bool IsWhite(byte input) => Classify(input) == CharacterClass.White;
    public static bool IsDelimiter(byte input) => Classify(input) == CharacterClass.Delimiter;
    public static bool IsRegular(byte input) => Classify(input) == CharacterClass.Regular;
}
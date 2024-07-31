namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal ref struct PredefinedEncodings(GlyphFromSid sidMapper)
{
    private readonly byte[] result = new byte[256];
    private int index = 0;

    public byte[] Standard()
    {
        Repeat(0, 32);
        Sequence(1, 95);
        Repeat(0, 34);
        Sequence(96,110);
        Item(0);
        Sequence(111,114);
        Item(0);
        Sequence(115,122);
        Item(0);
        Item(123);
        Item(0);
        Sequence(124,131);
        Item(0);
        Sequence(132, 133);
        Item(0);
        Sequence(134, 137);
        Repeat(0,16);
        Item(138);
        Item(0);
        Item(139);
        Repeat(0,4);
        Sequence(140,143);
        Repeat(0, 5);
        Item(144);
        Repeat(0, 3);
        Item(145);
        Repeat(0,2);
        Sequence(146,149);
        Repeat(0,4);
        return result;
    }

    private void Sequence(ushort low, ushort high)
    {
        for(var i = low; i <= high; i++)
        {
            Item(i);
        }
    }

    private void Repeat(ushort datum, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Item(datum);
        }
    }

    private void Item(ushort datum)
    {
        result[index++] = (byte)sidMapper.Search(datum);
    }

    public byte[] Expert()
    {
        Repeat(0,32);
        Item(1);
        Sequence(229,230);
        Item(0);
        Sequence(231, 238);
        Sequence(13,15);
        Item(99);
        Sequence(239, 248);
        Sequence(27,28);
        Sequence(249,252);
        Item(0);
        Sequence(253, 257);
        Repeat(0,3);
        Item(258);
        Repeat(0,2);
        Sequence(259, 262);
        Repeat(0,2);
        Sequence(263,265);
        Item(0);
        Item(266);
        Sequence(109,110);
        Sequence(267,269);
        Item(0);
        Sequence(270,303);
        Repeat(0, 34);
        Sequence(304,306);
        Repeat(0,2);
        Sequence(307,311);
        Item(0);
        Item(312);
        Repeat(0,2);
        Item(313);
        Repeat(0,2);
        Sequence(314,315);
        Repeat(0,2);
        Sequence(316,318);
        Repeat(0,3);
        Item(158);
        Item(155);
        Item(163);
        Sequence(319,325);
        Repeat(0,2);
        Item(326);
        Item(150);
        Item(164);
        Item(169);
        Sequence(327, 378);
        return result;
    }
    
}
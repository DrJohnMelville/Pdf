using System.Text;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal readonly struct CffStringIndex(CffIndex customStrings)
{
    public ValueTask<string> GetNameAsync(int index) =>
        index < Type1StandardStrings.Instance.Length? 
            new(Type1StandardStrings.Instance[index]):
            LoadCustomStringAsync(index - Type1StandardStrings.Instance.Length);

    private async ValueTask<string> LoadCustomStringAsync(int localIndex)
    {
        using var data = await customStrings.ItemDataAsync(localIndex).CA();
        return Encoding.UTF8.GetString(
            data.Sequence);
    }
}

internal ref struct StandardCharsetFactory
{
    private string[] result = [];
    private int index = 0;

    public StandardCharsetFactory()
    {
    }

    public string[] FromByte(long charSetOffset) => charSetOffset switch
    {
        0 => IsoAdobe(),
        1 => Expert(),
        2 => ExpertSubset(),
        _ => throw new InvalidDataException("Invalid standard charset idnex")
    };

    private string[] IsoAdobe()
    {
        CreateResult(229);
        AddSpan(0, 228);
        return result;
    }

    private void CreateResult(int length)
    {
        result = new string[length];
    }

    private void AddSpan(int minimum, int maximum)
    {
        for (int i = minimum; i <= maximum; i++)
        {
            AddSpan(i);
        }
    }

    private string AddSpan(int i) => result[index++] = Type1StandardStrings.Instance[i];

    private string[] Expert()
    {
        CreateResult(166);
        AddSpan(0,1);
        AddSpan(229,238);
        AddSpan(13,15);
        AddSpan(99);
        AddSpan(239,248);
        AddSpan(27,28);
        AddSpan(249,265);
        AddSpan(266);
        AddSpan(109, 110);
        AddSpan(267, 318);
        AddSpan(158);
        AddSpan(155);
        AddSpan(163);
        AddSpan(319,326);
        AddSpan(150);
        AddSpan(164);
        AddSpan(169);
        AddSpan(327, 378);
        return result;
    }

    private string[] ExpertSubset()
    {
        CreateResult(87);
        AddSpan(0, 1);
        AddSpan(231, 232);
        AddSpan(235, 238);
        AddSpan(13, 15);
        AddSpan(99);
        AddSpan(239, 248);
        AddSpan(27,28);
        AddSpan(249, 251);
        AddSpan(253, 266);
        AddSpan(109,110);
        AddSpan(267,270);
        AddSpan(272);
        AddSpan(300,302);
        AddSpan(305);
        AddSpan(314, 315);
        AddSpan(158);
        AddSpan(155);
        AddSpan(163);
        AddSpan(320,326);
        AddSpan(150);
        AddSpan(164);
        AddSpan(169);
        AddSpan(327, 346);
        return result;
    }

}
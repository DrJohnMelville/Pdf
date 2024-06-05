using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.INPC;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.CFFGlyphViewers;

public partial class CffGlyphViewModel
{
    [FromConstructor] private readonly CffGlyphSource GlpyhSource;
    [AutoNotify] private CffGlyphBuffer? renderedGlyph;
    [AutoNotify] private bool unitSquare = true;
    [AutoNotify] private bool boundingBox = true;
    [AutoNotify] private bool points = true;
    [AutoNotify] private bool controlPoints = true;
    [AutoNotify] private bool phantomPoints = true;
    [AutoNotify] private bool outline = true;
    [AutoNotify] private bool fill = false;
    public PageSelectorViewModel PageSelector { get; } = new();

    partial void OnConstructed()
    {
        PageSelector.MinPage = 0;
        PageSelector.MaxPage = GlpyhSource.GlyphCount;
        PageSelector.WhenMemberChanges(nameof(PageSelector.Page), LoadNewGlyph);
        LoadNewGlyph();
    }

    private async void LoadNewGlyph()
    {
        var renderTemp = new CffGlyphBuffer();
        await GlpyhSource.RenderGlyph((uint)PageSelector.Page, renderTemp);
        RenderedGlyph = renderTemp;
    }
}

public class CffGlyphBuffer : ICffGlyphTarget
{
    private readonly List<CffGlyphInstruction> instructions = new();

    public void Instruction(int instruction, ReadOnlySpan<DictValue> values) =>
        instructions.Add(new CffGlyphInstruction(instruction, values.ToArray()));

    public string ViewOutput => string.Join(Environment.NewLine, instructions.Select(i=>i.ToString()));
}

public record struct CffGlyphInstruction(int Instruction, DictValue[] Values)
{
    public override string ToString()
    {
        return $"{InstructionName} ({string.Join(", ", Values.Select(i=>i.FloatValue.ToString("#######0.###")))})";
    }

    public string InstructionName => Instruction switch
    {
        0 => "Reserved",
        1 => "hstem",
        2 => "Reserved",
        3 => "vstem",
        4 => "vmoveto",
        5 => "rlineto",
        6 => "hlineto",
        7 => "vlineto",
        8 => "rrcurveto",
        9 => "Reserved",
        10 => "callsubr",
        11 => "return",
        12 => "escape",
        13 => "Reserved",
        14 => "endchar",
        15 => "Reserved",
        16 => "blend",
        17 => "Reserved",
        18 => "hstemhm",
        19 => "hintmask",
        20 => "cntrmask",
        21 => "rmoveto",
        22 => "hmoveto",
        23 => "vstemhm",
        24 => "rcurveline",
        25 => "rlinecurve",
        26 => "vvcurveto",
        27 => "hhcurveto",
        28 => "shortint",
        29 => "callgsubr",
        30 => "vhcurveto",
        31 => "hvcurveto",
        0x0C00 => "Reserved",
        0x0C01 => "Reserved",
        0x0C02 => "Reserved",
        0x0C03 => "and",
        0x0C04 => "or",
        0x0C05 => "not",
        0x0C06 => "Reserved",
        0x0C07 => "Reserved",
        0x0C08 => "Reserved",
        0x0C09 => "abs",
        0x0C0A => "add",
        0x0C0B => "sub",
        0x0C0C => "div",
        0x0C0D => "Reserved",
        0x0C0E => "neg",
        0x0C0F => "eq",
        0x0C10 => "Reserved",
        0x0C11 => "Reserved",
        0x0C12 => "drop",
        0x0C13 => "Reserved",
        0x0C14 => "put",
        0x0C15 => "get",
        0x0C16 => "ifelse",
        0x0C17 => "random",
        0x0C18 => "mul",
        0x0C19 => "Reserved",
        0x0C1A => "sqrt",
        0x0C1B => "dup",
        0x0C1C => "exch",
        0x0C1D => "index",
        0x0C1E => "roll",
        0x0C1F => "Reserved",
        0x0C20 => "Reserved",
        0x0C21 => "Reserved",
        0x0C22 => "hflex",
        0x0C23 => "flex",
        0x0C24 => "hflex1",
        0x0C25 => "flex1",
        >= 0x0C26 and <= 0x0CFF => "Reserved",
        _ => "Unknown"
    };
}
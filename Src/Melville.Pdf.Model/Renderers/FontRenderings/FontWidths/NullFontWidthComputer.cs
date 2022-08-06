namespace Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;

public class NullFontWidthComputer : IFontWidthComputer
{
    public static NullFontWidthComputer Instance = new();

    private NullFontWidthComputer() { }
    public double GetWidth(uint character, double defaultWidth) => defaultWidth;
}
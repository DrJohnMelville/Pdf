namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public interface IFontWidthComputer
{
    double GetWidth(byte character, double defaultWidth);
}

public class NullFontWidthComputer : IFontWidthComputer
{
    public double GetWidth(byte character, double defaultWidth) => defaultWidth;
}

public class ExplicitFontWidthComputer : IFontWidthComputer
{
    private readonly byte firstChar;
    private readonly double[] widths;

    public ExplicitFontWidthComputer(byte firstChar, double[] widths)
    {
        this.firstChar = firstChar;
        this.widths = widths;
    }

    public double GetWidth(byte character, double defaultWidth) => 
        TryLookupWidth(character - firstChar, defaultWidth);

    private double TryLookupWidth(int value, double defaultWidth) => 
        value >= 0 && value < widths.Length ? widths[value] : defaultWidth;
}
namespace Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;
public interface IFontWidthComputer
{
    double GetWidth(uint character, double defaultWidth);
}
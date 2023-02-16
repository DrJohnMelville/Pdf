namespace Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;
internal interface IFontWidthComputer
{
    double GetWidth(uint character, double defaultWidth);
}
namespace Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;
internal interface IFontWidthComputer
{
    double? TryGetWidth(uint character);
}
using System.Threading.Tasks;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

public readonly struct FontWidthParser
{
    private readonly IRealizedFont innerFont;
    private readonly PdfFont pdfFont;
    private readonly double sizeFactor;

    public FontWidthParser(IRealizedFont innerFont, PdfFont pdfFont, double size)
    {
        this.innerFont = innerFont;
        this.pdfFont = pdfFont;
        sizeFactor = size /1000;
    }

    public ValueTask<IRealizedFont> Parse(int subTypeKey) => new (innerFont);
}
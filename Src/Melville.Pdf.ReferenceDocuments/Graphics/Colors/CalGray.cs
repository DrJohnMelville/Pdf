using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class CalGray: ColorBars
{
    public CalGray() : base("Four different shades of the CalGray profile")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ColorSpace, NameDirectory.Get("CS1"), new PdfArray(
            KnownNames.CalGray, new DictionaryBuilder().WithItem(KnownNames.WhitePoint, new PdfArray(
                0.9505, 1.000, 1.0890)).AsDictionary()));
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        await csw.SetStrokingColorSpace(NameDirectory.Get("CS1"));
        csw.SetLineWidth(15);
        DrawLine(csw);
        csw.SetStrokeColor(0.25);
        DrawLine(csw);
        csw.SetStrokeColor(0.5);
        DrawLine(csw);
        csw.SetStrokeColor(0.75);
        DrawLine(csw);
    }

}
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Text.Type3;

public class Type3Font : Type3FontBase
{
    public Type3Font() : base("Render a type 3 font")
    {
    }
}

public class Type3FontInvisible3 : Type3FontBase
{
    public Type3FontInvisible3() : base("Render a type 3 font in invisible mode 3")
    {
    }

    protected override ValueTask BetweenFontWritesAsync(ContentStreamWriter csw)
    {
        csw.SetTextRender(TextRendering.Invisible);
        return base.BetweenFontWritesAsync(csw);
    }
}
    public class Type3FontInvisible7 : Type3FontBase
{
    public Type3FontInvisible7() : base("Render a type 3 font in invisible mode 7")
    {
    }

    protected override ValueTask BetweenFontWritesAsync(ContentStreamWriter csw)
    {
        csw.SetTextRender(TextRendering.Clip);
        return base.BetweenFontWritesAsync(csw);
    }
}
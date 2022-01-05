namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public enum TextRendering
{
    Fill = 0,
    Stroke = 1,
    FillAndStroke = 2,
    Invisible = 3,
    FillAndClip = 4,
    StrokeAndClip = 5,
    FillStrokeAndClip = 6,
    Clip = 7
}

public static class TextRenderingOperations
{
    public static bool ShouldStroke(this TextRendering tr) => tr is
        TextRendering.Stroke or 
        TextRendering.FillAndStroke or 
        TextRendering.StrokeAndClip or 
        TextRendering.FillStrokeAndClip;

    public static bool ShouldFill(this TextRendering tr) => tr is
        TextRendering.Fill or 
        TextRendering.FillAndStroke or 
        TextRendering.FillAndClip or 
        TextRendering.FillStrokeAndClip;

    public static bool ShouldClip(this TextRendering tr) => tr is
        TextRendering.StrokeAndClip or 
        TextRendering.FillAndClip or 
        TextRendering.Clip or 
        TextRendering.FillStrokeAndClip;
}
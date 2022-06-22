using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public readonly struct ArithmeticBitmapReaderContext
{
    private readonly BitmapTemplate template;
    private readonly ContextStateDict dictionary;

    public ArithmeticBitmapReaderContext(BitmapTemplate template) : this()
    {
        this.template = template;
        dictionary = new ContextStateDict(this.template.BitsRequired());
    }

    public ref ContextEntry ReadContext(BinaryBitmap bitmap, int row, int col) =>
        ref dictionary.EntryForContext(template.ReadContext(bitmap, row, col));

    public ref ContextEntry GetContext(int index) =>
        ref dictionary.EntryForContext(index);

    public IncrementalTemplate ToIncrementalTemplate() => template.ToIncrementalTemplate();
}
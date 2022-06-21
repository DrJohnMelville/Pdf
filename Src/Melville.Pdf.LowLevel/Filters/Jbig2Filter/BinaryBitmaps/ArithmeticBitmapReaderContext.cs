using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public struct ArithmeticBitmapReaderContext
{
    private BitmapTemplate template;
    private readonly ContextStateDict dictionary;

    public ArithmeticBitmapReaderContext(BitmapTemplate template) : this()
    {
        this.template = template;
        dictionary = new ContextStateDict(this.template.BitsRequired());
    }

    public ref ContextEntry ReadContext(BinaryBitmap bitmap, int row, int col) =>
        ref dictionary.EntryForContext(template.OldReadContext(bitmap, row, col));

    public void ResetContext(IBinaryBitmap bitmap, int row, int col) =>
        template.ResetContext(bitmap, row, col);
    public ref ContextEntry CurrentContext(BinaryBitmap bitmap, int row, int col) =>
        ref dictionary.EntryForContext(template.contextValue);

    public ref ContextEntry GetContext(ushort index) =>
        ref dictionary.EntryForContext(index);
}


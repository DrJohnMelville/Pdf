using Melville.JBig2.ArithmeticEncodings;

namespace Melville.JBig2.BinaryBitmaps;

public readonly struct ArithmeticBitmapReaderContext
{
    private readonly BitmapTemplate template;
    private readonly ContextStateDict dictionary;

    public ArithmeticBitmapReaderContext(BitmapTemplate template) : this()
    {
        this.template = template;
        dictionary = new ContextStateDict(this.template.BitsRequired());
    }

    public ref ContextEntry GetContext(int index) =>
        ref dictionary.EntryForContext(index);

    public IncrementalTemplate ToIncrementalTemplate() => template.ToIncrementalTemplate();
}
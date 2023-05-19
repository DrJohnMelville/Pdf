using System.Numerics;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal interface IBitmapFilter
{
    unsafe byte AlphaForByte(byte* target, int row, int col);
}

internal readonly partial struct MaskedFilter<TMaskType>: 
    IBitmapFilter where TMaskType : IMaskType
{
    [FromConstructor]private readonly MaskBitmap mask;
    [FromConstructor]private readonly TMaskType maskOperation;
    [FromConstructor]private readonly int destHeight;
    [FromConstructor]private readonly int destWidth;

    public unsafe byte AlphaForByte(byte* target, int row, int col)
    {
        return maskOperation.AlphaForByte(target[3], 
            mask.PixelAt(row*mask.Height/destHeight, col*mask.Width/destWidth));
    }
}

using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public class MaskedBitmapWriter : IComponentWriter
{
    private readonly IComponentWriter innerWriter;
    private readonly MaskBitmap mask;
    private readonly double widthDelta;
    private readonly double heightDelta;
    private double row;
    private double col;


    public MaskedBitmapWriter(IComponentWriter innerWriter, MaskBitmap mask,
        int parentWidth, int parentHeight)
    {
        this.innerWriter = innerWriter;
        this.mask = mask;
        widthDelta = (double)mask.Width / (parentWidth-1);
        heightDelta = (double)mask.Height / (parentHeight-1);
        col = 0;
        row = mask.Height - heightDelta;
    }

    public int ColorComponentCount => innerWriter.ColorComponentCount;
    
    public unsafe void WriteComponent(ref byte* target, int[] component)
    {
        if (mask.ShouldWrite((int)row, (int)col))
            innerWriter.WriteComponent(ref target, component);
        else
            BitmapPointerMath.PushPixel(ref target, DeviceColor.Invisible);
        FindNextPixel();
    }

    private void FindNextPixel()
    {
        col += widthDelta;
        if (col < mask.Width) return;
        NextRow();
    }

    private void NextRow()
    {
        row -= heightDelta;
        col = 0;
    }
}

public readonly struct MaskBitmap
{
    public byte[] Mask { get; }
    public int Width { get; }
    public int Height { get; }
                                        
    public MaskBitmap(byte[] mask, int width, int height)
    {
        Mask = mask;
        Width = width;
        Height = height;
    }

    public bool ShouldWrite(int row, int col) =>
        255 == Mask[3 + (4 * (col + (row * Width)))];

    public static async ValueTask<MaskBitmap> Create(PdfStream stream, IHasPageAttributes page)
    {
        var wrapped = await PdfBitmapOperatons.WrapForRenderingAsync(stream, page, DeviceColor.Black);
        var buffer = new byte[wrapped.ReqiredBufferSize()];
        await FillBuffer(buffer, wrapped);
        return new MaskBitmap(buffer, wrapped.Width, wrapped.Height);
    }

    private static unsafe ValueTask FillBuffer(byte[] buffer, IPdfBitmap wrapped)
    {
        fixed (byte* pointer = buffer)
        {
            return wrapped.RenderPbgra(pointer);
        }
    }
}
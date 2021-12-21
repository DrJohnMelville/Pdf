using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public partial class MaskedBitmapWrapper : IPdfBitmap
{
    [DelegateTo()] private readonly IPdfBitmap reader;
    private readonly MaskBitmap maskImage;

    public MaskedBitmapWrapper(IPdfBitmap reader, MaskBitmap maskImage)
    {
        this.reader = reader;
        this.maskImage = maskImage;
    }

    public unsafe ValueTask RenderPbgra(byte* buffer) => RenderAsync(new BitmapMaskOperation(this, buffer));

    private async ValueTask RenderAsync(BitmapMaskOperation wrapper)
    {
        await wrapper.BaseRender();
        wrapper.DoMasking();
    }

    private unsafe void DoMasking(byte* target)
    {
        var widthDelta = maskImage.Width / (double)reader.Width;
        var heightDelta = maskImage.Height / (double)reader.Height;
        var maskY = 0.0;
        for (int i = 0; i < reader.Height; i++)
        {
            var maskX = 0.0;
            for (int j = 0; j < reader.Width; j++)
            {
                FilterSinglePixel(ref target, (int)maskX, (int)maskY);
                maskX += widthDelta;
            }

            maskY += heightDelta;
        }
    }

    private unsafe void FilterSinglePixel(ref byte* target, int maskX, int maskY)
    {
        if (maskImage.ShouldWrite(maskY, maskX))
        {
            target += 4;
        } else
        {
            BitmapPointerMath.PushPixel(ref target, DeviceColor.Invisible);
        }
    }

    public unsafe readonly struct BitmapMaskOperation
    {
        private readonly MaskedBitmapWrapper parent;
        private readonly byte* target;

        public BitmapMaskOperation(MaskedBitmapWrapper parent, byte* target)
        {
            this.parent = parent;
            this.target = target;
        }

        public ValueTask BaseRender() => parent.reader.RenderPbgra(target);
        public void DoMasking() => parent.DoMasking(target);
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

    public bool ShouldWrite(int row, int col) => 255 == Mask[3 + (4 * (col+(row*Width)))];

    public static async ValueTask<MaskBitmap> Create(PdfStream stream, PdfPage page)
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
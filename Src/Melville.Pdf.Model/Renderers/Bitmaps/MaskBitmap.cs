using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public readonly struct MaskBitmap
{
    private byte[] Mask { get; }
    public int Width { get; }
    public int Height { get; }

    private MaskBitmap(byte[] mask, int width, int height)
    {
        Mask = mask;
        Width = width;
        Height = height;
    }

    public bool ShouldWrite(int row, int col) => 255 == AlphaAt(row, col);

    private byte AlphaAt(int row, int col) => Mask[3 + PixelPosition(row,col)];
    public byte RedAt(int row, int col) => Mask[PixelPosition(row, col)];

    private int PixelPosition(int row, int col) => 4 * (col + (row * Width));

    public static async ValueTask<MaskBitmap> Create(PdfStream stream, IHasPageAttributes page)
    {
        var wrapped = await PdfBitmapOperatons.WrapForRenderingAsync(stream, page, DeviceColor.Black).CA();
        var buffer = new byte[wrapped.ReqiredBufferSize()];
        await FillBuffer(buffer, wrapped).CA();
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
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public abstract class MaskedBitmapWriter : IComponentWriter
{
    private readonly IComponentWriter innerWriter;
    protected MaskBitmap Mask { get; }
    private readonly double widthDelta;
    private readonly double heightDelta;
    private double row;
    private double col;


    public MaskedBitmapWriter(IComponentWriter innerWriter, MaskBitmap mask,
        int parentWidth, int parentHeight)
    {
        this.innerWriter = innerWriter;
        this.Mask = mask;
        widthDelta = (double)mask.Width / (parentWidth-1);
        heightDelta = (double)mask.Height / (parentHeight-1);
        col = 0;
        row = mask.Height - heightDelta;
    }

    public int ColorComponentCount => innerWriter.ColorComponentCount;
    
    public unsafe void WriteComponent(ref byte* target, int[] component, byte alpha)
    {
        innerWriter.WriteComponent(ref target, component,AlphaForByte(alpha, row, col));
        FindNextPixel();
    }

    protected abstract byte AlphaForByte(byte alpha, double atRow, double atCol);

    private void FindNextPixel()
    {
        col += widthDelta;
        if (col < Mask.Width) return;
        NextRow();
    }

    private void NextRow()
    {
        row -= heightDelta;
        col = 0;
    }
}

public class HardMaskedBitmapWriter: MaskedBitmapWriter
{
    public HardMaskedBitmapWriter(
        IComponentWriter innerWriter, MaskBitmap mask, int parentWidth, int parentHeight) : 
        base(innerWriter, mask, parentWidth, parentHeight)
    {
    }

    protected override byte AlphaForByte(byte alpha, double atRow, double atCol) =>
        (byte)(Mask.ShouldWrite((int)atRow, (int)atCol) ? alpha : 0);
}
public class SoftMaskedBitmapWriter: MaskedBitmapWriter
{
    public SoftMaskedBitmapWriter(
        IComponentWriter innerWriter, MaskBitmap mask, int parentWidth, int parentHeight) : 
        base(innerWriter, mask, parentWidth, parentHeight)
    {
    }

    protected override byte AlphaForByte(byte alpha, double atRow, double atCol) =>
        Mask.RedAt((int)atRow, (int)atCol);
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

    public bool ShouldWrite(int row, int col) => 255 == AlphaAt(row, col);

    public byte AlphaAt(int row, int col) => Mask[3 + (4 * (col + (row * Width)))];
    public byte RedAt(int row, int col) => Mask[(4 * (col + (row * Width)))];

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
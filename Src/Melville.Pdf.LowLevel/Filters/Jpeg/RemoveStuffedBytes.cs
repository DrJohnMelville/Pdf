using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public partial class RemoveStuffedBytes : IAsyncByteSource
{
    private byte nextByte;
    [FromConstructor] private readonly IAsyncByteSource innerSource;

    public async ValueTask Initialize()
    {
        await innerSource.Initialize().CA();
        await GetByte().CA();
    }

    public async ValueTask<byte> GetByte()
    {
        (var ret, nextByte) = (nextByte, await innerSource.GetByte().CA());
        if (IsStuffedByte(ret))
        {
            nextByte = await innerSource.GetByte().CA();
            ret = 0xFF;
        }
        return ret;
    }

    private bool IsStuffedByte(byte ret) => ret == 0 && nextByte == 0xFF;
}
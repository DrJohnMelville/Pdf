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
        var priorWasFF = nextByte == 0xff;
        nextByte = await innerSource.GetByte().CA();
        if (CurrentByteIsStuffedByte(priorWasFF))
        {
            nextByte = await innerSource.GetByte().CA();
        }
        return nextByte;
    }

    private bool CurrentByteIsStuffedByte(bool priorWasFF) => priorWasFF && nextByte == 0;
}
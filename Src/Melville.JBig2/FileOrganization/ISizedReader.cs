using Melville.INPC;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Threading.Tasks;

namespace Melville.JBig2.FileOrganization;

internal interface ISizedReader
{
    ValueTask<uint> ReadAsync(ParsingBuffer buff);
}

[StaticSingleton]
internal partial class OneByteReader : ISizedReader
{
    public async ValueTask<uint> ReadAsync(ParsingBuffer buff) => await buff.GetUInt8Async();
}
[StaticSingleton]
internal partial class TwoByteReader : ISizedReader
{
    public async ValueTask<uint> ReadAsync(ParsingBuffer buff) => await buff.GetUInt16BigEndianAsync();
}
[StaticSingleton]
internal partial class FourByteReader : ISizedReader
{
    public ValueTask<uint> ReadAsync(ParsingBuffer buff) => buff.GetUInt32BigEndianAsync();
}

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
public partial class OneByteReader : ISizedReader
{
    public async ValueTask<uint> ReadAsync(ParsingBuffer buff) => await buff.GetUInt8Async();
}
[StaticSingleton]
public partial class TwoByteReader : ISizedReader
{
    public async ValueTask<uint> ReadAsync(ParsingBuffer buff) => await buff.GetUInt16BigEndianAsync();
}
[StaticSingleton]
public partial class FourByteReader : ISizedReader
{
    public ValueTask<uint> ReadAsync(ParsingBuffer buff) => buff.GetUInt32BigEndianAsync();
}

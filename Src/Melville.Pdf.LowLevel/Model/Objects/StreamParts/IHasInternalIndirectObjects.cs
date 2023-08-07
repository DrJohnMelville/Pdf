using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Model.Objects.StreamParts;

internal interface IInternalObjectTarget
{
    public ValueTask DeclareObjectStreamObjectAsync(
        int objectNumber, int streamObjectNumber, int streamOrdinal, int streamOffset);
}

internal readonly partial struct InternalObjectTargetForStream
{
    [FromConstructor] private readonly IInternalObjectTarget target;
    [FromConstructor] private readonly int streamOuterNumber;

    public ValueTask ReportObjectAsync(int objectNumber, int ordinal, int streamOffset) =>
        target.DeclareObjectStreamObjectAsync(objectNumber, streamOuterNumber, ordinal, streamOffset);
}

internal interface IHasInternalIndirectObjects
{
    ValueTask RegisterInternalObjectsAsync(InternalObjectTargetForStream target);
}
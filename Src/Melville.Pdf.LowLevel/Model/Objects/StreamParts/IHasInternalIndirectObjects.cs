using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Model.Objects.StreamParts;

internal interface IHasInternalIndirectObjects
{
    ValueTask<IEnumerable<ObjectLocation>> GetInternalObjectNumbersAsync();
}
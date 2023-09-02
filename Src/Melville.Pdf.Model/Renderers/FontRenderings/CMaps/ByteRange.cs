using System;
using System.Collections.Generic;
using System.IO;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps
{
    [FromConstructor]
    internal partial class ByteRange : CMapMapperBase
    {
        private readonly List<CMapMapperBase> mappers = new();

        public ByteRange(VariableBitChar minValue, VariableBitChar maxValue,
            params CMapMapperBase[] mappers) 
            : this(minValue, maxValue)
        {
            this.mappers.AddRange(mappers);
        }

        public void AddMapper(CMapMapperBase mapper) => mappers.Add(mapper);

        public override int WriteMapping(in VariableBitChar character, Memory<uint> target)
        {
            AssertValidRange();

            for (int i = mappers.Count -1; i >= 0; i--)
            {
                var candidate = mappers[i];
                if (candidate.AppliesTo(character)) 
                    return candidate.WriteMapping(character, target);
            }
            return WriteNotDefinedMapping(target.Span);
        }

        private void AssertValidRange()
        {
            if (mappers.Count == 0)
                throw new InvalidDataException("Cannot have an empty byte range");
        }

        private static int WriteNotDefinedMapping(Span<uint> target)
        {
            target[0] = 0;
            return 1;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext
{
    public class IndirectObjectResolver : IIndirectObjectResolver
    {
        private readonly Dictionary<(int, int), PdfIndirectReference> index = new();

        public IReadOnlyDictionary<(int, int), PdfIndirectReference> GetObjects() =>
            index;

        public PdfIndirectReference FindIndirect(int objectNumber, int generation)
        {
            if (index.TryGetValue((objectNumber, generation), out var existingReference)) 
                return existingReference;
            throw new InvalidDataException($"Cannot find indirect object: ({objectNumber}, {generation})");
        }

        public void AddLocationHint(int number, int generation, Func<ValueTask<PdfObject>> valueAccessor)
        {
            index.Add((number, generation), new PdfIndirectReference(new PdfIndirectObject(number, generation, valueAccessor)));
        }
    }
}
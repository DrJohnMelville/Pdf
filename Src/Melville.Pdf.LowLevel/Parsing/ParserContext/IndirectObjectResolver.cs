using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext
{
    public class IndirectObjectResolver : IIndirectObjectResolver
    {
        private readonly Dictionary<(int, int), PdfIndirectReference> index = new();
        public PdfIndirectReference FindIndirect(int objectNumber, int generation)
        {
            if (index.TryGetValue((objectNumber, generation), out var existingReference)) 
                return existingReference;
            var newReference = new PdfIndirectReference(new PdfIndirectObject(objectNumber, generation));
            index.Add((objectNumber, generation), newReference);
            return newReference;
        }

        public void AddLocationHint(int number, int generation, long location)
        {
            
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public interface IIndirectObjectResolver
{
    IReadOnlyDictionary<(int, int), PdfIndirectObject> GetObjects();
    PdfIndirectObject FindIndirect(int number, int generation);
    void AddLocationHint(PdfIndirectObject newItem);
}

public interface IIndirectObjectRegistry
{
    void RegisterDeletedBlock(int number, ulong next, ulong generation);
    void RegistedNullObject(int number, ulong next, ulong generation);
    void RegisterIndirectBlock(int number, ulong generation, ulong offset);
    void RegisterObjectStreamBlock(int number, ulong referredStreamOrdinal, ulong positionInStream);
}
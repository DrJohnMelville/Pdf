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
    Task<long> FreeListHead();
}

public interface IIndirectObjectRegistry
{
    void RegisterDeletedBlock(int number, int next, int generation);
    void RegistedNullObject(int number, int next, int generation);
    void RegisterIndirectBlock(int number, long generation, long offset);
    void RegisterObjectStreamBlock(int number, long referredStreamOrdinal, long referredStreamGeneration);
}
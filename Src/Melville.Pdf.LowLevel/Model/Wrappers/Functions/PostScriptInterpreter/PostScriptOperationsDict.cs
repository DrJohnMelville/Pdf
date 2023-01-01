using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

internal static class PostScriptOperationsDict
{
    private static readonly IReadOnlyDictionary<uint, IPostScriptOperation> operationsDict = 
            PostScriptOperations.CreateDictionary();

    public static IPostScriptOperation GetOperation(uint hash)
    {
        if (operationsDict.TryGetValue(hash, out var ret)) return ret;
        throw new PdfParseException("Unknown postscript operator");
    }
}
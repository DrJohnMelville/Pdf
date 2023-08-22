using System.Collections.Generic;
using Melville.INPC;
using Microsoft.CodeAnalysis;

namespace Melville.Postscript.OperationGenerator
{
    public readonly partial struct CodeGenerator
    {
        [FromConstructor] private readonly ISymbol classSymbol;
        [FromConstructor] private readonly IEnumerable<GeneratorAttributeSyntaxContext> methods;

        public string CreateCode()
        {
            return "// Fake code implementation";
        }
    }
}
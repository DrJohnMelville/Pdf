using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Postscript.OperationGenerator;

[Generator]
public class OperationGenerator: IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.SyntaxProvider.ForAttributeWithMetadataName("Melville.Postscript.Interpreter.FunctionLibrary.PostscriptMethodAttribute",
            (i,_)=> i is MethodDeclarationSyntax, (i,_) => i)
            .Collect()
            .SelectMany((i,_) =>
                i.GroupBy(j=>j.TargetSymbol.ContainingType, SymbolEqualityComparer.Default))
            .Select((j, _) => j), Generate);
    }

    private void Generate(SourceProductionContext context, IGrouping<ISymbol?, GeneratorAttributeSyntaxContext> classToGenerateFor)
    {
        if (classToGenerateFor.Key is {} key)
        context.AddSource(key.Name+".g.cs", new CodeGenerator(key, classToGenerateFor).CreateCode() );
    }
}
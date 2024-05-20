using System;
using System.Linq;
using Melville.INPC;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Net.Sockets;

namespace Melville.Fonts.TableGenerator;

[Generator]
public class TableGenerator: IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
         context.RegisterSourceOutput(context.SyntaxProvider.ForAttributeWithMetadataName(
                "Melville.Fonts.SfntParsers.TableParserParts.SFntFieldAttribute",
                (i,_)=> true, (i,_) => i)
            .Collect()
            .SelectMany((i,_) =>
                i.GroupBy(j=>j.TargetSymbol.ContainingType, SymbolEqualityComparer.Default))
            .Select((j, _) => j), Generate);
    }

    private void Generate(SourceProductionContext context, IGrouping<ISymbol?, GeneratorAttributeSyntaxContext> classToGenerateFor)
    {
        if (classToGenerateFor.Key is { } key)
        {
            var code = new CodeGenerator(key, classToGenerateFor).CreateCode();
            context.AddSource(key.Name+".g.cs", code );
        }
    }
}
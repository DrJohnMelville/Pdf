using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchitectureAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DocumentVisibleMembersAnalyzer:DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(Verifier.CheckMember, 
            SymbolKind.Event, SymbolKind.Field, SymbolKind.Method, SymbolKind.NamedType,
            SymbolKind.Property
            );
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        DocumentVisibleMembersDiagnostics.All;
}
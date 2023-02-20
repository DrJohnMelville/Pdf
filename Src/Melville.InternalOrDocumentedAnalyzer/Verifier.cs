using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchitectureAnalyzer;

public static class Verifier
{
    public static void CheckMember(SymbolAnalysisContext context)
    {
        if (!IsValidDeclaration(context.Symbol))
            context.ReportDiagnostic(CreateDiagnostic(context.Symbol));
    }

    private static bool IsValidDeclaration(ISymbol symbol)
    {
        return SymbolNotExternallyVisible(symbol) ||
               HasXmlDocComment(symbol) ||
               RecursiveIsNonvisibleSymbol(symbol.ContainingType);
    }

    private static bool HasXmlDocComment(ISymbol symbol) => 
        !string.IsNullOrEmpty(symbol.GetDocumentationCommentXml());

    private static bool RecursiveIsNonvisibleSymbol(INamedTypeSymbol? type) =>
        type is not null &&
        (SymbolNotExternallyVisible(type) ||
         RecursiveIsNonvisibleSymbol(type.ContainingType));

    private static bool SymbolNotExternallyVisible(ISymbol symbol)
    {
        return HasExplicitInternalVisibility(symbol) ||
               IsInternalMember(symbol);
    }

    private static bool IsInternalMember(ISymbol symbol)
    {
        return !(symbol.CanBeReferencedByName ||
                 IsConstructor(symbol) ||
                 IsIndexer(symbol));
    }

    private static bool IsConstructor(ISymbol symbol) => symbol is IMethodSymbol{MethodKind: MethodKind.Constructor};

    private static bool IsIndexer(ISymbol symbol) =>
        symbol is IPropertySymbol { IsIndexer: true };

    private static bool HasExplicitInternalVisibility(ISymbol symbol)
    {
        return symbol.DeclaredAccessibility is
            Accessibility.Internal or
            Accessibility.ProtectedAndInternal or
            Accessibility.Private;
    }

    private static Diagnostic CreateDiagnostic(ISymbol symbol) =>
        Diagnostic.Create(
            DocumentVisibleMembersDiagnostics.RuleViolated,
            FirstIdentifier(symbol.DeclaringSyntaxReferences.First().GetSyntax()).GetLocation(),
            $"{symbol.Name} is visible outside the assembly and does not have an Xml comment");

    private static SyntaxNodeOrToken FirstIdentifier(SyntaxNode node) =>
        node
            .DescendantNodesAndTokens()
            .Where(i => i.IsKind(SyntaxKind.IdentifierToken))
            .DefaultIfEmpty(node)
            .First();
}
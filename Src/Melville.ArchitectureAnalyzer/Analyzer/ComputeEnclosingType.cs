using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchitectureAnalyzer.Analyzer;

public static class ComputeEnclosingType
{
    public static string? EnclosingTypeName(this SyntaxNodeAnalysisContext context) =>
        context.EnclosingTypeDecl()?.ToString();
    public static ITypeSymbol? EnclosingTypeDecl(this SyntaxNodeAnalysisContext context) =>
        EnclosingType(context.Node) is {} decl ? 
            DeclaredTypeSymbol(decl,context.SemanticModel): null;

    private static ITypeSymbol? DeclaredTypeSymbol(TypeDeclarationSyntax typeDecl, SemanticModel sm) =>
        sm.GetDeclaredSymbol(typeDecl);

    private static TypeDeclarationSyntax? EnclosingType(SyntaxNode? syntaxNode) =>
        syntaxNode switch
        {
            null => null,
            TypeDeclarationSyntax tds => tds,
            _ => EnclosingType(syntaxNode.Parent)
        };

}
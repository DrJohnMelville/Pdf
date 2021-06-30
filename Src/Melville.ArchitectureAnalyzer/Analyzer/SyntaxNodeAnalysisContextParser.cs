using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchitectureAnalyzer.Analyzer
{
    public readonly struct SyntaxNodeAnalysisContextParser
    {
        private readonly SyntaxNodeAnalysisContext context;

        public SyntaxNodeAnalysisContextParser(SyntaxNodeAnalysisContext context)
        {
            this.context = context;
        }
        
        public string? ReferencedTypeName() =>
            TypeSyntax(context.Node) is not {} typeSyntax? 
                null: FullTypeName(typeSyntax);

        private TypeSyntax? TypeSyntax(SyntaxNode node) =>
            node switch
            {
                TypeSyntax ts => ts,
                _ => null
            };

        private string? FullTypeName(TypeSyntax typeSyntax) => 
            FindTypeForSymbol(context.SemanticModel.GetSymbolInfo(typeSyntax).Symbol)?.ToString();

        private ISymbol? FindTypeForSymbol(ISymbol? symbol)
        {
            return symbol switch
            {
                IMethodSymbol ms => ms.ReturnType,
                _=>symbol
            };
        }


        public string? EnclosingTypeName() =>
            EnclosingType(context.Node) is {} decl ? DeclaredTypeName(decl): null;

        private string? DeclaredTypeName(TypeDeclarationSyntax typeDecl) =>
            context.SemanticModel.GetDeclaredSymbol(typeDecl)?.ToString();

        private TypeDeclarationSyntax? EnclosingType(SyntaxNode? syntaxNode) =>
            syntaxNode switch
            {
                null => null,
                TypeDeclarationSyntax tds => tds,
                _ => EnclosingType(syntaxNode.Parent)
            };
    }
}
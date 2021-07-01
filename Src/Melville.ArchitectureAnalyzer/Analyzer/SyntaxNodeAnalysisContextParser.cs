using System.Linq;
using ArchitectureAnalyzer.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchitectureAnalyzer.Analyzer
{
    public readonly struct SyntaxNodeAnalysisContextParser
    {
        private readonly SyntaxNodeAnalysisContext context;
        private readonly IDependencyRules rules;
        private readonly ITypeSymbol? location;

        public SyntaxNodeAnalysisContextParser(
            SyntaxNodeAnalysisContext context, IDependencyRules rules)
        {
            this.context = context;
            this.rules = rules;
            location = null;
            location = EnclosingTypeName();
        }

        public void CheckReference()
        {
            if (location == null) return;
            ReferencedTypeName();
        }

        private void ReferencedTypeName()
        {
            TypeSyntax(context.Node);
        }

        private void TypeSyntax(SyntaxNode node)
        {
            if (node is TypeSyntax ts)
                    FullTypeName(ts);
        }

        private void FullTypeName(TypeSyntax typeSyntax) => 
            FindTypeForSymbol(context.SemanticModel.GetSymbolInfo(typeSyntax).Symbol);

        private void FindTypeForSymbol(ISymbol? symbol)
        {
            switch (symbol)
            {
                case IMethodSymbol ms:
                    CheckMethodCall(ms);
                    break;
                case ITypeSymbol ts:
                    TestSymbolReference(ts);
                    break;
            }
        }

        private void CheckMethodCall(IMethodSymbol ms)
        {
            TestSymbolReference(ms.ReturnType);
            foreach (var paramType in ms.Parameters.Select(i => i.Type))
            {
                TestSymbolReference(paramType);
            }
        }


        #region Find Enclosing Type

        public ITypeSymbol? EnclosingTypeName() =>
            EnclosingType(context.Node) is {} decl ? DeclaredTypeName(decl): null;

        private ITypeSymbol? DeclaredTypeName(TypeDeclarationSyntax typeDecl) =>
            context.SemanticModel.GetDeclaredSymbol(typeDecl);

        private TypeDeclarationSyntax? EnclosingType(SyntaxNode? syntaxNode) =>
            syntaxNode switch
            {
                null => null,
                TypeDeclarationSyntax tds => tds,
                _ => EnclosingType(syntaxNode.Parent)
            };

        #endregion
        
        private void TestSymbolReference(ITypeSymbol? usedType)
        {
            if (usedType is not {SpecialType: SpecialType.None}) return;
            if ( VerifyReference(usedType) is {} errorMessage) 
            {
                context.ReportDiagnostic(Diagnostic.Create(DependencyDiagnostics.RuleViolated,
                    context.Node.GetLocation(), errorMessage));
            }
        }

        private string? VerifyReference(ITypeSymbol usedType) =>
            (location?.ToString() is {} locStr && usedType.ToString() is {} useStr )  ? 
                rules.ErrorFromReference(locStr, useStr):null;
    }
}
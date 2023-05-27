using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Rename;

namespace Melville.AsyncAnalyzer
{
    public static class RenameHelper
    {
        public static async Task<Solution> RenameSymbolAsync(
            Microsoft.CodeAnalysis.Document document, SyntaxNode root, 
            SyntaxToken declarationToken, string newName, 
            CancellationToken cancellationToken)
        {
            var annotatedRoot = 
                root.ReplaceToken(declarationToken, 
                    declarationToken.WithAdditionalAnnotations(RenameAnnotation.Create()));
            var annotatedSolution = document.Project.Solution.WithDocumentSyntaxRoot(
                document.Id, annotatedRoot);
            var annotatedDocument = annotatedSolution.GetDocument(document.Id);

            if (annotatedDocument is null) return annotatedSolution;

            annotatedRoot = await annotatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (annotatedRoot is null) return annotatedSolution;
            var annotatedToken = annotatedRoot.FindToken(declarationToken.SpanStart);
            if (annotatedToken.Parent is null) return annotatedSolution;
            var semanticModel = await annotatedDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var symbol = semanticModel?.GetDeclaredSymbol(annotatedToken.Parent, cancellationToken);
            if (symbol == null) return annotatedSolution;

            var newSolution = await Renamer.RenameSymbolAsync(
                annotatedSolution, symbol, 
                new SymbolRenameOptions(false, false, false, false), newName, cancellationToken).ConfigureAwait(false);
            return newSolution;
        }
    }
}
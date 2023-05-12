using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.ImageExtractor
{
    public class UncoloredGraphicsState : GraphicsState
    {
        protected override void StrokeColorChanged()
        {
        }

        protected override void NonstrokeColorChanged()
        {
        }

        public override ValueTask SetStrokePattern(
            PdfDictionary pattern, DocumentRenderer parentRenderer) =>
            ValueTask.CompletedTask;

        public override ValueTask SetNonstrokePattern(
            PdfDictionary pattern, DocumentRenderer parentRenderer) => 
            ValueTask.CompletedTask;
    }
}
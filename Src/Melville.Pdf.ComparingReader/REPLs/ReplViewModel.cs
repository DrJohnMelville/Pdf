using Melville.INPC;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.ReferenceDocuments.Graphics;

namespace Melville.Pdf.ComparingReader.REPLs;

public partial class ReplViewModel
{
    private readonly IMultiRenderer renderer;

    [AutoNotify] private string contentStreamText ="";

    private async void OnContentStreamTextChanged(string newValue)
    {
        var target = new MultiBufferStream();
        await new RelpPage(newValue).WritePdfAsync(target);
        renderer.SetTarget(target);
    }

    public ReplViewModel(IMultiRenderer renderer)
    {
        this.renderer = renderer;
    }
    
    private class RelpPage: Card3x5
    {
        private readonly string contentStream;
        public RelpPage(string contentStream) : base("Content of the Repl Dialog")
        {
            this.contentStream = contentStream;
        }

        protected override void DoPainting(ContentStreamWriter csw)
        {
            csw.WriteLiteral(contentStream);
        }
    }
}
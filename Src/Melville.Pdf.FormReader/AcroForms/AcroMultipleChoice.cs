using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Melville.INPC;
using Melville.Pdf.FormReader.Interface;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.FormReader.AcroForms;

[FromConstructor]
internal partial class AcroMultipleChoice : AcroPick, IPdfMultiPick
{
    [FromConstructor] private readonly ObservableCollection<PdfPickOption> selected;
    public IList<PdfPickOption> Selected => selected;

    partial void OnConstructed()
    {
        selected.CollectionChanged += UpdateValue;
    }

    private void UpdateValue(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Value = PdfDirectObject.FromArray(selected.Select(i=>(PdfIndirectObject)i.Value).ToArray());
    }
}
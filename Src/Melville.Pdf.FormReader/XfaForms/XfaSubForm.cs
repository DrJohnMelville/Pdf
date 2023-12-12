using System.Xml.Linq;
using Melville.INPC;
using Melville.Pdf.FormReader.AcroForms;

namespace Melville.Pdf.FormReader.XfaForms;

internal partial class XfaSubForm: IXfaControl
{
    [FromConstructor] private readonly string name;
    public ReadOnlySpan<char> LocalName => NameSplitter.LocalName(name);
    private readonly List<IXfaControl>  controls = new ();
    public void AddControl(IXfaControl? control)
    {
        if (!(control is null  || NameAlreadyDefined(control)) ) controls.Add(control);
    }

    private bool NameAlreadyDefined(IXfaControl control) => controls.Any(i=>i.LocalName.SequenceEqual(control.LocalName));

    public void WriteValues(XElement dataSet)
    {
        foreach (var child in dataSet.Elements())
        {
            var name = child.Name.LocalName;
            if (SearchForChild(name) is {} childControl)
                childControl.WriteValues(child);
        }
    }

    private IXfaControl? SearchForChild(string name)
    {
        return controls.FirstOrDefault(i=>i.LocalName.SequenceEqual(name.AsSpan()));
    }

    public string ChildControlName(string childName) => name.Length == 0 ? childName : $"{name}.{childName}";

    public object?[] DataElements() => controls
        .Select(i =>(object?) i.GetSingleDataItems()).ToArray();

    public XElement GetSingleDataItems() => 
        new(LocalName.ToString(), DataElements());
}
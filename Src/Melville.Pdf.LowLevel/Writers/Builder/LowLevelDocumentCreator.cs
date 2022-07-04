using System.Collections.Generic;
using System.Linq;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public interface ILowLevelDocumentCreator: ILowLevelDocumentBuilder
{
    PdfLowLevelDocument CreateDocument();
    void SetVersion(byte major, byte minor);
}

public partial class LowLevelDocumentCreator: ILowLevelDocumentCreator
{
    private byte major = 1;
    private byte minor = 7;
    private readonly LowLevelDocumentBuilder data;

    public LowLevelDocumentCreator(LowLevelDocumentBuilder? data = null)
    {
        this.data = data ?? new LowLevelDocumentBuilder();
    }

    [DelegateTo]
    private ILowLevelDocumentBuilder Builder => data;
    
    public void SetVersion(byte major, byte minor)
    {
        this.major = major;
        this.minor = minor;
    }

    public PdfLowLevelDocument CreateDocument() => 
        new(major, minor, data.CreateTrailerDictionary(), CreateObjectList());

    private Dictionary<(int, int), PdfIndirectObject> CreateObjectList() => 
        data.Objects.ToDictionary(item => (item.ObjectNumber, item.GenerationNumber));
}
namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;

public class ReferencePartViewModel:DocumentPart
{
    public CrossReference RefersTo { get; }

    public ReferencePartViewModel(string prefix, int objectNumber, int generationNumber) : 
        base($"{prefix}{objectNumber} {generationNumber} R")
    {
        RefersTo = new CrossReference(objectNumber, generationNumber);
    }

    public override object? DetailView => this;
}

public record struct CrossReference(int Object, int Generation);
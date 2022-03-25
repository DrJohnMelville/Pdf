namespace Melville.Pdf.Model.Renderers;

public readonly struct OptionalContentCounter
{
    private readonly uint count;

    public OptionalContentCounter(uint count)
    {
        this.count = count;
    }
}
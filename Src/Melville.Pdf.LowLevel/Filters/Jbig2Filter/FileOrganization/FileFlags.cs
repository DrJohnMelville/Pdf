using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.FileOrganization;

public readonly struct FileFlags
{
    private readonly byte data;

    public FileFlags(byte data)
    {
        this.data = data;
    }

    public bool SequentialFileOrganization => BitOperations.CheckBit(data, 1);
    public bool UnknownPageCount => BitOperations.CheckBit(data, 2);
}
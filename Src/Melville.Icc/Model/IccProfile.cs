using System.Buffers;
namespace Melville.Icc.Model;

public class IccProfile
{
    public IccProfile(IccHeader header)
    {
        Header = header;
    }

    public IccHeader Header { get; }

}
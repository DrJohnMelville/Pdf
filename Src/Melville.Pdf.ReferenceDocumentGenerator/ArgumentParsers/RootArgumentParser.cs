using System.Threading.Tasks;
using Melville.Pdf.ReferenceDocumentGenerator.Targets;

namespace Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

public interface IRootParser
{
    ValueTask ParseAsync(string argument);
    ITarget Target { get; set; }
}
public class RootArgumentParser: IRootParser
{
    private readonly IArgumentParser defaultParser;
    private IArgumentParser nextParser;
    public ITarget Target { get; set; } = new ViewTarget();

    public RootArgumentParser(IArgumentParser defaultParser)
    {
        this.nextParser = this.defaultParser = defaultParser;
    }
    public async ValueTask ParseAsync(string argument) => 
        nextParser = (await nextParser.ParseArgumentAsync(argument, this)) ?? defaultParser;
}
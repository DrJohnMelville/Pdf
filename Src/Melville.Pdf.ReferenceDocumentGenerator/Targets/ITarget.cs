using System.IO;
using System.Threading.Tasks;

namespace Melville.Pdf.ReferenceDocumentGenerator.Targets;

public interface ITarget
{
    public Stream CreateTargetStream();
    public void View();
}
using System.IO;

namespace Melville.Pdf.ReferenceDocumentGenerator.Targets;

public interface ITarget
{
    public Stream CreateTargetStream();
    public void View();
}
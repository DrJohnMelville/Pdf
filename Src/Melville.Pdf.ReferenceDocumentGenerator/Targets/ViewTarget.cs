using System;
using System.Diagnostics;
using System.IO;

namespace Melville.Pdf.ReferenceDocumentGenerator.Targets
{
    public class ViewTarget: ITarget
    {
        private string path;

        public ViewTarget()
        {
            path = Path.Join(Path.GetTempPath(), Guid.NewGuid()+".pdf");
        }

        public Stream CreateTargetStream() => File.Create(path);

        public void View()
        {
            var process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = true, FileName = path,
            };
            process.Start();
            Console.WriteLine("Waiting for exit");
            process.WaitForExit();
            Console.WriteLine("Deleting file.");
            File.Delete(path);
        }
    }
}
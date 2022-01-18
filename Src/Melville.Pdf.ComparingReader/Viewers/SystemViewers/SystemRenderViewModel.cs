using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Melville.INPC;
using Melville.Pdf.ComparingReader.Renderers;

namespace Melville.Pdf.ComparingReader.Viewers.SystemViewers;

public partial class SystemRenderViewModel: IRenderer
{
    public string DisplayName => "Shell Viewer";
    public object RenderTarget => this;
    [AutoNotify] private string message = "";
    private Stream? savedTarget;

    public void SetTarget(Stream pdfBits, int _)
    {
        savedTarget = pdfBits;
    }

    public async Task OpenFile()
    {
        Message = "Saving";
        var name = await WriteToUniqueFile();
        if (name is null)
        {
            Message = "No file to Write";
            return;
        }
        Message = "Opening";
        var process = LaunchViewer(name);
        Message = "Waiting for exit";
        await DeleteFileWhenDone(process, name);
    }

    private async Task<string?> WriteToUniqueFile()
    {
        if (savedTarget == null) return null;
        var name = UniquePdfName();
        await using (var file = File.OpenWrite(name))
        {
            savedTarget.Seek(0, SeekOrigin.Begin);
            await savedTarget.CopyToAsync(file);
        }

        return name;
    }

    private Process LaunchViewer(string name)
    {
        var process = new Process();
        process.EnableRaisingEvents = true;
        process.StartInfo = new ProcessStartInfo()
        {
            UseShellExecute = true, FileName = name,
        };
        process.Start();
        return process;
    }

    private async ValueTask DeleteFileWhenDone(Process process, string name)
    {
        await process.WaitForExitAsync();
        await Task.Delay(500);
        while (true)
        {
            try
            {
                Message = "Deleting file.";
                File.Delete(name);
                return;
            }
            catch (Exception)
            {
                Message = "Retrying";
                await Task.Delay(1000);
            }
        }
    }

    private string UniquePdfName() => Path.Join(Path.GetTempPath(), Guid.NewGuid() + ".pdf");
}
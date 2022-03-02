using Melville.MVVM.WaitingServices;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer;

public class FakeWaitingService: IWaitingService, IDisposable
{
    public void MakeProgress(string? item = null)
    {
    }

    public double Total { get; set; }

    public double Progress { get; set; }

    public string? ProgressMessage { get; set; }

    public CancellationToken CancellationToken => default;

    public IDisposable WaitBlock(string message, double maximum = Double.MinValue, bool showCancelButton = false)
    {
        return this;
    }

    public string? WaitMessage { get; set; }

    public string? ErrorMessage { get; set; }
    public void Dispose() { }
}
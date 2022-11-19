using Melville.Pdf.LowLevel.Model.ContentStreams;

namespace Melville.Pdf.Model.Renderers.ColorOperations;

//PDF spec 2.0 clause 8.6.8 dictates color operators which are turned off at certian times
public class SwitchingColorStrategy
{
    private readonly IColorOperations passthrough;
    public IColorOperations CurrentTarget { get; private set; }

    public SwitchingColorStrategy(IColorOperations passthrough)
    {
        this.passthrough = passthrough;
        this.CurrentTarget = passthrough;
    }

    public void TurnOff() => CurrentTarget = NullColorOperations.Instance;
    public void TurnOn() => CurrentTarget = passthrough;
}
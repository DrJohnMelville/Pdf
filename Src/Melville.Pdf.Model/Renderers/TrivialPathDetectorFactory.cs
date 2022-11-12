using Melville.Parsing.ObjectRentals;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;

public static class TrivialPathDetectorFactory
{
    private static readonly ObjectRentalManager<TrivialPathDetector> rental = new(10);

    public static TrivialPathDetector Wrap(IGraphicsState state, IDrawTarget inner) =>
        rental.Rent().With(inner, state);

    public static void Return(TrivialPathDetector item) => rental.Return(item);
}

internal enum TrivialPathDetectorState
{
    Start, InitialMoveTo, MustDraw, ShouldNotDraw
}
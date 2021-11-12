namespace Melville.Pdf.ReferenceDocuments.Infrastructure;

public static class GeneratorFactory
{
    public static IEnumerable<IPdfGenerator> AllGenerators =>
        typeof(IPdfGenerator).Assembly.GetTypes()
            .Where(IsGeneratorType())
            .Select(CreateWithDefaultConstructor);

    private static Func<Type, bool> IsGeneratorType() => i => 
        i != typeof(CreatePdfParser) && i.IsAssignableTo(typeof(IPdfGenerator)) && !i.IsAbstract;

    private static IPdfGenerator CreateWithDefaultConstructor(Type i) => 
        (IPdfGenerator)(Activator.CreateInstance(i) ?? 
                                                throw new InvalidOperationException("Cannot Create: " + i));

}
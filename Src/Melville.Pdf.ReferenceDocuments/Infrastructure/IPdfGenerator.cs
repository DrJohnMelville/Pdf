namespace Melville.Pdf.ReferenceDocuments.Infrastructure
{
    public interface IPdfGenerator
    {
        public string Prefix { get; }
        public string HelpText { get; }
        public string Password { get; }
        public ValueTask WritePdfAsync(Stream target);
    }
}
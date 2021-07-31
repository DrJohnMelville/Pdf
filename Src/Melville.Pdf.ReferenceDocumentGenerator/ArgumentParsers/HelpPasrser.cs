using System;
using System.Threading.Tasks;

namespace Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers
{
    public class HelpPasrser: IArgumentParser
    {
        public string Prefix => "-h";

        public ValueTask<IArgumentParser?> ParseArgumentAsync(string argument, IRootParser root)
        {
            Console.WriteLine();
            Console.WriteLine("Help");
            Console.WriteLine("----");
            Console.WriteLine(" -H        Display this string");
            Console.WriteLine(" -V        View the output with default pdf viewer. (default)");
            Console.WriteLine(" -F <path> Set output to a file");
            Console.WriteLine(" -V        View the generated file immediately");
            Console.WriteLine("");
            Console.WriteLine("  -Min  Generate a one page blank pdf");
            Console.WriteLine("  -Filters  Generate a document using all the filter types");
            
            return ValueTask.FromResult((IArgumentParser?) null);
        }
    }
}
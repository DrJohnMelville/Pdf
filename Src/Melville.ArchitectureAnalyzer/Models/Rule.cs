using System.Linq;
using System.Linq.Expressions;

namespace ArchitectureAnalyzer.Models
{
    public class Rule
    {
        public string? ErrorMessage { get; }
        public bool Allowed => ErrorMessage == null;
        public Rule(string declaration, bool allowed)
        {
            ErrorMessage = allowed?null: declaration;
        }
    }
}
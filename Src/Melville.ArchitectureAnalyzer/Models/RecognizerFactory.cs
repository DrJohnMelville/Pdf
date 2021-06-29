using System.Collections.Generic;
using System.Linq;
using ArchitectureAnalyzer.Parsers;

namespace ArchitectureAnalyzer.Models
{
    public readonly struct RecognizerFactory
    {
        private readonly Dictionary<string, GlobRecognizer> dict;

        public RecognizerFactory(Dictionary<string, GlobRecognizer> dict)
        {
            this.dict = dict;
        }

        public GlobRecognizer Create(string item) => 
            dict.Memorized(item, i => new GlobRecognizer(item));

        public IEnumerable<GlobRecognizer> MatchingRecognizers(string item) =>
            dict.Values.Where(i => i.Matches(item));

        public void DeclareGroup(string groupName, IEnumerable<string> groupMembers)
        {
            if (dict.ContainsKey(groupName))
                throw new DslException($"Duplicate group definition \"{groupName}\"");
            dict.Add(groupName, new GlobRecognizer(groupMembers));
        }
    }
}
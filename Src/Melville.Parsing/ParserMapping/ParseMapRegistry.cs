namespace Melville.Parsing.ParserMapping
{
    internal static class ParseMapRegistry
    {
        private static readonly Lock mutex = new();
        private static List<ParseMap> maps = new();
        public static ParseMap NewMap(object? source)
        {
            var ret = new ParseMap();
            ret.AddAlias(source);
            lock (mutex)
            {
                maps.Add(ret);
            }
            return ret;
        }

        public static void LogToMap(object key, string label, int streamPosition)
        {
            lock (mutex)
            {
                FindMap(key)?.AddEntry(label, streamPosition);
            }
        }

        private static ParseMap? FindMap(object key) => maps.FirstOrDefault(i => i.MonitoringKey(key));

        public static void Remove(ParseMap parseMap)
        {
            lock (mutex)
            {
                maps.Remove(parseMap);
            }
        }

        public static void AddAlias(object key, object alias)
        {
            lock (mutex)
            {
                FindMap(key)?.AddAlias(alias);
            }
        }

        public static void Indent(object alias, string title)
        {
            lock (mutex)
            {
                FindMap(alias)?.Indent(title);
            }
        }

        public static void Outdent(object alias)
        {
            lock (mutex)
            {
                FindMap(alias)?.Outdent();
            }
        }

        public static void PeerIndent(object alias, string title)
        {
            lock (mutex)
            {
                if (FindMap(alias) is not {} map) return;
                map.Outdent();
                map.Indent(title);
            }
        }
    }
}
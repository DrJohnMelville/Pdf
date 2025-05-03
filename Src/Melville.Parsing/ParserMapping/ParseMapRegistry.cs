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


        public static IParseMap? FindMap(object? key) => key switch
        {
            null => null,
            IParseMap ret => ret,
            _ => SearchForMap(key)
        };

        private static ParseMap? SearchForMap(object key)
        {
            lock (mutex)
            {
                return maps.FirstOrDefault(i => i.MonitoringKey(key));
            }
        }


        public static void Remove(ParseMap parseMap)
        {
            lock (mutex)
            {
                maps.Remove(parseMap);
            }
        }

    }
}
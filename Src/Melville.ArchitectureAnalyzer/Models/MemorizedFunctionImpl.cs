using System;
using System.Collections.Generic;

namespace ArchitectureAnalyzer.Models
{
    public static class MemorizedFunctionImpl
    {
        public static TRet Memorized<TArgs, TRet>(this IDictionary<TArgs, TRet> store, TArgs args,
            Func<TArgs, TRet> creator)
        {
            if (store.TryGetValue(args, out var ret)) return ret;
            var newRet = creator(args);
            store.Add(args, newRet);
            return newRet;
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProcessor;

internal static class FilesToProcess
{
    public static ConcurrentDictionary<string, string> Files = new();
}

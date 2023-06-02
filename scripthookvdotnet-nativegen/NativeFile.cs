using System.Collections.Generic;
using NetJSON;
// ReSharper disable ClassNeverInstantiated.Global

namespace NativeGen
{
    public sealed class NativeFile : Dictionary<string, NativeNamespace>
    {
    }

	public sealed class NativeNamespace : Dictionary<string, NativeFunction>
    {
    }

    public sealed class NativeFunction
    {
        [NetJSONProperty("name")]
        public string Name { get; set; }

        [NetJSONProperty("params")]
        public List<NativeParams> Params { get; set; }

        [NetJSONProperty("results")]
        public string Results { get; set; }

        [NetJSONProperty("jhash")]
        public string JHash { get; set; }
    }

    public sealed class NativeParams
    {
        [NetJSONProperty("type")]
        public string Type { get; set; }

        [NetJSONProperty("name")]
        public string Name { get; set; }
    }
}
using System.Collections.Generic;
using Newtonsoft.Json;
// ReSharper disable ClassNeverInstantiated.Global

namespace NativeGen
{
    public class NativeFile : Dictionary<string, NativeNamespace>
    {
    }

    public class NativeNamespace : Dictionary<string, NativeFunction>
    {
    }

    public class NativeFunction
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("params")]
        public List<NativeParams> Params { get; set; }

        [JsonProperty("results")]
        public string Results { get; set; }

        [JsonProperty("jhash")]
        public string JHash { get; set; }
    }

    public class NativeParams
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
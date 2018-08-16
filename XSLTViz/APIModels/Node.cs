using Newtonsoft.Json;

namespace XSLTViz.APIModels
{
    [JsonObject]
    public class Node
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "fixed")]
        public bool IsFixed { get; set; }
        
        [JsonProperty(PropertyName = "x")]
        public double? X { get; set; }

        [JsonProperty(PropertyName = "y")]
        public double? Y { get; set; }

        [JsonProperty(PropertyName = "leaf")]
        public bool IsLeaf { get; set; }

        public Node() { }
    }
}
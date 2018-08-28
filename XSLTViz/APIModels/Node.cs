using Newtonsoft.Json;
using XSLTViz.DataModel;

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

        [JsonProperty(PropertyName = "size")]
        public long Size { get; set; }
        
        [JsonProperty(PropertyName = "x")]
        public double? X { get; set; }

        [JsonProperty(PropertyName = "y")]
        public double? Y { get; set; }

        [JsonProperty(PropertyName = "leaf")]
        public bool IsLeaf { get; set; }

        public Node() { }

        public Node(File file)
        {
            Id = file.Id;
            Name = file.Path;

            if (file.Point.X != null && file.Point.Y != null)
            {
                X = file.Point.X;
                Y = file.Point.Y;
                IsFixed = file.IsFixed;
            }
        }
    }
}
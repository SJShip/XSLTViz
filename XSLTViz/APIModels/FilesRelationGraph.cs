using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XSLTViz.APIModels
{
    public class Graph
    {
        [JsonProperty(PropertyName = "links")]
        public List<Link> Links { get; set; }

        [JsonProperty(PropertyName = "nodes")]
        public List<Node> Nodes { get; set; }
    }
}
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

	public class Path
	{
		[JsonProperty(PropertyName = "links")]
		public List<Link> Links { get; set; }

		[JsonProperty(PropertyName = "nodes")]
		public List<int> Nodes { get; set; }
	}

	public class Tree
	{
		[JsonProperty(PropertyName = "nodes")]
		public List<TreeNode> Nodes { get; set; }

		[JsonProperty(PropertyName = "links")]
		public List<TreeLink> Links { get; set; }
	}
}
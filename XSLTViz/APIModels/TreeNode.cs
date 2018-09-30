using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XSLTViz.DataModel;

namespace XSLTViz.APIModels
{
	[JsonObject]
	public class TreeNode
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "children")]
		public List<string> Imports { get; set; }

		public TreeNode() { }

		public TreeNode(File file)
		{
			Id = file.Id;
			Name = file.Path;
		}
	}
}
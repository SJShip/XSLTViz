using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XSLTViz.APIModels
{
	public class TreeLink
	{
		[JsonProperty(PropertyName = "source")]
		public TreeNode Source { get; set; }

		[JsonProperty(PropertyName = "target")]
		public TreeNode Target { get; set; }
	}
}
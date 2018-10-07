using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XSLTViz.DataModel;

namespace XSLTViz.APIModels
{
	public class TemplateNode: INode
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		public TemplateNode() { }

		public TemplateNode(Template template)
		{
			Id = template.Id;
			Name = template.Selector == TemplateSelector.Name? String.Format("Name={0}", template.SelectorValue): String.Format("Match={0}", template.SelectorValue);
		}
	}
}
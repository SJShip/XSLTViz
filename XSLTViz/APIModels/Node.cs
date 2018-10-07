using Newtonsoft.Json;
using System;
using XSLTViz.DataModel;

namespace XSLTViz.APIModels
{
	public interface INode
	{
		[JsonProperty(PropertyName = "name")]
		string Name { get; set; }

		[JsonProperty(PropertyName = "id")]
		int Id { get; set; }
	}

	[JsonObject]
	public class Node : INode
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
			Size = (long)Math.Ceiling(Convert.ToDecimal(file.Content.Length / 4000)) + 1;

			if (file.Point.X != null && file.Point.Y != null)
			{
				X = file.Point.X;
				Y = file.Point.Y;
				IsFixed = file.IsFixed;
			}
		}

		public Node(Template template)
		{
			Id = template.Id;
			Name = template.Selector == TemplateSelector.Name ? String.Format("Name={0}", template.SelectorValue) : String.Format("Match={0}", template.SelectorValue);

			if (template.Mode.Length > 0)
			{
				Name = Name + string.Format("[{0}]", template.Mode);
			}
		}
	}
}
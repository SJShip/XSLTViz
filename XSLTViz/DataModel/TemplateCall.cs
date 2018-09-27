using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace XSLTViz.DataModel
{
	public class TemplateCall
	{
		public int Id { get; set; }

		[Required]
		public TemplateSelector Selector { get; set; }

		public string SelectorValue { get; set; }

		public bool ForImports { get; set; }

		public string Mode { get; set; }

		public Template Template { get; set; }
	}
}
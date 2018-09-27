using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XSLTViz.DataModel
{
	public class TemplatesRelation
	{
		public int Id { get; set; }

		public Template Source { get; set; }

		public Template Target { get; set; }
	}
}
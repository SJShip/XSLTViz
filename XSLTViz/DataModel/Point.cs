using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace XSLTViz.DataModel
{
    [ComplexType]
    public class Point
    {
        [JsonProperty(PropertyName = "x")]
        public double? X { get; set; }

        [JsonProperty(PropertyName = "y")]
        public double? Y { get; set; }
    }
}
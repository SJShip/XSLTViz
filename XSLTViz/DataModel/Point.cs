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
        public Point() {
            X = 1;
            Y = 1;
        }
        [JsonProperty(PropertyName = "x")]
        public int? X { get; set; }

        [JsonProperty(PropertyName = "y")]
        public int? Y { get; set; }
    }
}
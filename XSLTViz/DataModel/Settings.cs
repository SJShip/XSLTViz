using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace XSLTViz.DataModel
{
    [ComplexType]
    [JsonObject]
    public class Settings
    {
        [JsonProperty(PropertyName = "highlighted_leafs")]
        public bool AreLeafsHighlighted { get; set; }

        [JsonProperty(PropertyName = "color_direction")]  
        public bool IsDirectionColored { get; set; }

        [JsonProperty(PropertyName = "viewbox")]
        public string ViewBox { get; set; }
    }
}
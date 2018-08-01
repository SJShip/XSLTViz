using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XSLTViz.APIModels
{
    public class Link
    {
        [JsonProperty(PropertyName = "source")]
        public int Source { get; set; }

        [JsonProperty(PropertyName = "target")]
        public int Target { get; set; }
    }
}
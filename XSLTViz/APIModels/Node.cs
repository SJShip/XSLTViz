using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XSLTViz.DataModel;

namespace XSLTViz.APIModels
{
    public class Node: Point
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "fixed")]
        public bool IsFixed { get; set; }
    }
}
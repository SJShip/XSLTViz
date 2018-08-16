using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace XSLTViz.DataModel
{
    public class Project
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "projectName")]
        [Required]
        public string ProjectName { get; set; }

        [JsonProperty(PropertyName = "settings")]
        public Settings Settings { get; set; }
    }
}
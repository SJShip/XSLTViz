using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace XSLTViz.DataModel
{
    public enum TemplateSelector
    {
        Name,
        Match
    }

    public class Template
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public TemplateSelector Selector { get; set; }

        [Required]
        public string Mode { get; set; }

        [Required]
        public TemplateFile File { get; set; }
    }
}
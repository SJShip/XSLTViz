using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace XSLTViz.DataModel
{
    public class TemplateFile
    {
        public int Id { get; set; }

        [Required]
        public string FilePath { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
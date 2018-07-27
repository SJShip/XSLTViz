using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace XSLTViz.DataModel
{
    public class File
    {
        public int Id { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public Project Project { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace XSLTViz.DataModel
{
    public class Project
    {
        public Project()
        {
            this.Files = new List<TemplateFile>();
        }

        public int Id { get; set; }
        [Required]
        public string ProjectName { get; set; }

        public IList<TemplateFile> Files;
    }
}
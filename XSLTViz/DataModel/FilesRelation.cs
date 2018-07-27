using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace XSLTViz.DataModel
{
    public enum FilesRelationMode {
        Include,
        Import
    }

    public class FilesRelation
    {
        public int Id { get; set; }

        public File Source { get; set; }

        public File Target { get; set; }

        [Required]
        public FilesRelationMode Mode {get; set;}
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XSLTViz.DataModel;

namespace XSLTViz.APIModels
{
    public class ProjectInfo: Project
    {
        public ProjectInfo() { }

        public ProjectInfo(Project project, int totalFiles)
        {
				this.Id = project.Id - 1;
            this.Settings = project.Settings;
            this.TotalFiles = totalFiles;
            this.ProjectName = project.ProjectName;
        }

        [JsonProperty(PropertyName = "totalFiles")]
        public int TotalFiles { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace XSLTViz.DataModel
{
    public class TestDBInitializer: DropCreateDatabaseAlways<DataContext>
    {
        protected override void Seed(DataContext context)
        {
            var testProject = context.Projects.Add(new Project { ProjectName="TestProject", Settings = new Settings { } });
            var filesLocation = ConfigurationManager.AppSettings["filesLocation"];
            var files = Directory.GetFiles(filesLocation);
            foreach (var filePath in files)
            {
                if (filePath.EndsWith(".xsl") || filePath.EndsWith(".xslt"))
                {
                    var content = System.IO.File.ReadAllText(filePath);
                    var lIndex = filePath.LastIndexOf("\\");
                    var shortPath = filePath.Substring(lIndex + 1);
                    context.Files.Add(new File { Content = content, Path = shortPath, Project = testProject, Point = new Point() });
                }
            }
            context.SaveChanges();

            ParseFactory.ParseProject(context, testProject);


            base.Seed(context);
        }
    }
}
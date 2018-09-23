using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace XSLTViz.DataModel
{
    public static class ParseFactory
    {
        public static void ParseFile(DataContext context, File file, int projectId)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(file.Content);

            var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("xsl", "http://www.w3.org/1999/XSL/Transform");

            var importNodes = xmlDoc.SelectNodes("//xsl:import", nsmgr);
            foreach (XmlNode importNode in importNodes)
            {
                var importPath = importNode.Attributes["href"].Value;
                context.FilesRelations.Add(new FilesRelation { Source = context.Files.Local.First(f => f.Path.ToLower() == importPath.ToLower() 
					 && f.Project.Id == projectId),
                    Target = file, Mode = FilesRelationMode.Import});
            }

            var includeNodes = xmlDoc.SelectNodes("//xsl:include", nsmgr);
            foreach (XmlNode includeNode in includeNodes)
            {
                var includePath = includeNode.Attributes["href"].Value;
                context.FilesRelations.Add(new FilesRelation { Source = context.Files.Local.First(f => f.Path.ToLower() == includePath.ToLower()
					 && f.Project.Id == projectId),
                    Target = file, Mode = FilesRelationMode.Include });
            }
        }

        public static void ParseProject(DataContext context, Project project)
        {
            var files = (from f in context.Files
                        where f.Project.Id == project.Id
                        select f).ToList();
 

            foreach (File file in files)
            {
                ParseFile(context, file, project.Id);
            }
            context.SaveChanges();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using XSLTViz.APIModels;

namespace XSLTViz.DataModel
{
	public class TestDBInitializer : DropCreateDatabaseAlways<DataContext>
	{
		protected override void Seed(DataContext context)
		{

			// Default project
			var defaultProject = context.Projects.Add(new Project { ProjectName = "All Stylesheets", Settings = new Settings { AreLabelsShown = true, IsDirectionColored = true, AreLeafsHighlighted = true} });
			var filesLocation = ConfigurationManager.AppSettings["filesLocation"];
			var files = Directory.GetFiles(filesLocation);
			foreach (var filePath in files)
			{
				if (filePath.EndsWith(".xsl") || filePath.EndsWith(".xslt"))
				{
					var content = System.IO.File.ReadAllText(filePath);
					var lIndex = filePath.LastIndexOf("\\");
					var shortPath = filePath.Substring(lIndex + 1);
					context.Files.Add(new File { Content = content, Path = shortPath, Project = defaultProject, Point = new Point() });
				}
			}
			context.SaveChanges();
			ParseFactory.ParseProject(context, defaultProject);
			ParseFactory.GenerateTemplateRelations(context, defaultProject.Id);

			// Separate views for *.xsl files
			//foreach (var filePath in files)
			//{
			//	if (filePath.EndsWith(".xsl"))
			//	{
			//		var lIndex = filePath.LastIndexOf("\\");
			//		var shortPath = filePath.Substring(lIndex + 1);

			//		var newProject = context.Projects.Add(new Project { ProjectName = shortPath, Settings = new Settings { AreLabelsShown = true, IsDirectionColored = true, AreLeafsHighlighted = true } });

			//		var nodes = new List<int>();


			//		var file = (from f in context.Files
			//						where f.Path == shortPath && f.Project.Id == defaultProject.Id
			//						select f).FirstOrDefault();

			//		var stack = new Stack<int>();

			//		if (file != null)
			//		{
			//			stack.Push(file.Id);
			//		}

			//		while (stack.Count > 0)
			//		{
			//			int current = stack.Pop();

			//			if (nodes.IndexOf(current) == -1)
			//			{
			//				nodes.Add(current);
			//			}

			//			var neighbors = (from fr in context.FilesRelations
			//								  where fr.Target.Id == current
			//								  select fr.Source).ToList();
			//			foreach (var n in neighbors)
			//			{
			//				stack.Push(n.Id);
			//			}
			//		}

			//		foreach (var fileNode in nodes)
			//		{
			//			var data = (from f in context.Files
			//								where f.Id == fileNode
			//								select f).FirstOrDefault();


			//			context.Files.Add(new File { Content = data.Content, Path = data.Path, Project = newProject, Point = new Point() });
			//		}
			//		context.SaveChanges();
			//		ParseFactory.ParseProject(context, newProject);
			//	}
			//}

			base.Seed(context);
		}
	}
}
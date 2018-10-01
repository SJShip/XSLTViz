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
			var defaultProject = context.Projects.Add(new Project { ProjectName = "All Stylesheets", Settings = new Settings { } });
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

			// Separate views for *.xsl files
			foreach (var filePath in files)
			{
				if (filePath.EndsWith(".xsl"))
				{
					var lIndex = filePath.LastIndexOf("\\");
					var shortPath = filePath.Substring(lIndex + 1);

					var newProject = context.Projects.Add(new Project { ProjectName = shortPath, Settings = new Settings { } });

					var nodes = new List<Node>();


					var file = (from f in context.Files
									where f.Path == shortPath && f.Project.Id == defaultProject.Id
									select f).FirstOrDefault();

					var stack = new Stack<Node>();

					if (file != null)
					{
						var node = new Node(file);
						stack.Push(node);
					}

					while (stack.Count > 0)
					{
						Node current = stack.Pop();

						if (nodes.IndexOf(current) == -1)
						{
							nodes.Add(current);
						}

						var neighbors = (from fr in context.FilesRelations
											  where fr.Target.Id == current.Id
											  select fr.Source).ToList();
						foreach (var n in neighbors)
						{
							var node = new Node(n);
							stack.Push(node);
						}
					}

					foreach (var fileNode in nodes)
					{
						var content = (from f in context.Files
											where f.Path == fileNode.Name
											select f.Content).FirstOrDefault();

						context.Files.Add(new File { Content = content, Path = fileNode.Name, Project = newProject, Point = new Point() });
					}
					context.SaveChanges();
					ParseFactory.ParseProject(context, newProject);
				}
			}

			base.Seed(context);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace XSLTViz.DataModel
{
	public static class ParseFactory
	{
		public static void ParseTemplateData(DataContext context, XmlNode templateNode, Template template, XmlNamespaceManager nsMgr)
		{
			var callList = new List<TemplateCall>();
			var callsByName = templateNode.SelectNodes("//xsl:call-template", nsMgr);
			for (var i = 0; i < callsByName.Count; i++)
			{
				XmlNode call = callsByName[i];

				var name = call.Attributes["name"] != null ? call.Attributes["name"].Value : "";
				var mode = call.Attributes["mode"] != null ? call.Attributes["mode"].Value : "";

				if (!callList.Any(t => t.Selector == TemplateSelector.Name && t.SelectorValue == name && t.Mode == mode))
				{
					callList.Add(new TemplateCall
					{
						Mode = mode,
						Selector = TemplateSelector.Name,
						SelectorValue = name,
						Template = template
					});
				}
			}

			var applyCalls = templateNode.SelectNodes("//xsl:apply-templates", nsMgr);
			for (var i = 0; i < applyCalls.Count; i++)
			{
				XmlNode call = applyCalls[i];

				var match = call.Attributes["match"] != null ? call.Attributes["match"].Value : "";
				var mode = call.Attributes["mode"] != null ? call.Attributes["mode"].Value : "";

				if (!callList.Any(t => t.Selector == TemplateSelector.Match && t.SelectorValue == match && t.Mode == mode))
				{
					callList.Add(new TemplateCall
					{
						Mode = mode,
						Selector = TemplateSelector.Match,
						SelectorValue = match,
						Template = template
					});
				}
			}

			var applyImportCalls = templateNode.SelectNodes("//xsl:apply-imports", nsMgr);
			for (var i = 0; i < applyImportCalls.Count; i++)
			{
				XmlNode call = applyImportCalls[i];

				var match = call.Attributes["match"] != null ? call.Attributes["match"].Value : "";
				var mode = call.Attributes["mode"] != null ? call.Attributes["mode"].Value : "";

				if (!callList.Any(t => t.Selector == TemplateSelector.Match && t.SelectorValue == match && t.ForImports && t.Mode == mode))
				{
					callList.Add(new TemplateCall {
						Mode = mode,
						Selector = TemplateSelector.Match,
						SelectorValue = match,
						ForImports = true,
						Template = template
					});
				}
			}

			if (callList.Count > 0)
			{
				context.TemplateCalls.AddRange(callList);
			}
		}

		public static void ParseFile(DataContext context, File file, int projectId)
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(file.Content);

			var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
			nsmgr.AddNamespace("xsl", "http://www.w3.org/1999/XSL/Transform");

			var importNodes = xmlDoc.SelectNodes("//xsl:import", nsmgr);
			for (var i = 0; i < importNodes.Count; i++)
			{
				XmlNode importNode = importNodes[i];
				var importPath = importNode.Attributes["href"].Value;
				context.FilesRelations.Add(new FilesRelation
				{
					Source = context.Files.Local.First(f => f.Path.ToLower() == importPath.ToLower()
						&& f.Project.Id == projectId),
					Target = file,
					Order = i,
					Mode = FilesRelationMode.Import
				});
			}

			var includeNodes = xmlDoc.SelectNodes("//xsl:include", nsmgr);
			for (var i = 0; i< includeNodes.Count; i++)
			{
				XmlNode includeNode = includeNodes[i];
				var includePath = includeNode.Attributes["href"].Value;
				context.FilesRelations.Add(new FilesRelation
				{
					Source = context.Files.Local.First(f => f.Path.ToLower() == includePath.ToLower()
						&& f.Project.Id == projectId),
					Target = file,
					Order = i,
					Mode = FilesRelationMode.Include
				});
			}

			var templates = xmlDoc.SelectNodes("//xsl:template", nsmgr);
			foreach (XmlNode templateNode in templates)
			{
				var match = templateNode.Attributes["match"] != null? templateNode.Attributes["match"].Value: "";
				var name = templateNode.Attributes["name"] != null ? templateNode.Attributes["name"].Value: "";
				var mode = templateNode.Attributes["mode"] !=null? templateNode.Attributes["mode"].Value: "";

				TemplateSelector selector = match.Length > 0 ? TemplateSelector.Match : TemplateSelector.Name;
				var template = new Template
				{
					Content = templateNode.InnerXml,
					File = file,
					Mode = mode,
					Selector = selector,
					SelectorValue = selector == TemplateSelector.Match ? match : name
				};
				context.Templates.Add(template);

				ParseTemplateData(context, templateNode, template, nsmgr);
			}
		}

		public static void ProcessTemplates(DataContext context, int fileId)
		{
			var importedTemplates = GetImportedTemplates(context, fileId);
			var includedTemplates = GetIncludedTemplates(context, fileId);

			var fileTemplates = (from t in context.Templates
										where t.File.Id == fileId
										select t).ToList();

			List<TemplateCall> fileTemplateCalls = new List<TemplateCall>();

			foreach (var t in fileTemplates)
			{
				var tCalls = (from call in context.TemplateCalls
												  where call.Template.Id == t.Id
												  select call).ToList();
				fileTemplateCalls.AddRange(tCalls);
			}

			
		}

		public static void GenerateTemplateRelations(DataContext context, int projectId)
		{
			var entryPoints = (from f in context.Files
													 where f.Project.Id == projectId && f.Path.EndsWith(".xsl")
													 select f).ToList();

			foreach (var file in entryPoints)
			{
				ProcessTemplates(context, file.Id);
			}
			context.SaveChanges();
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

			GenerateTemplateRelations(context, project.Id);
		}
	}
}
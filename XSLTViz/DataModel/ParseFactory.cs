using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace XSLTViz.DataModel
{
	public static class ParseFactory
	{
		public static List<int> ProcessedFiles;
		public static Dictionary<int, List<Template>> TemplateScope;


		static ParseFactory()
		{
			ProcessedFiles = new List<int>();
			TemplateScope = new Dictionary<int, List<Template>>();
		}

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
					callList.Add(new TemplateCall
					{
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
			for (var i = 0; i < includeNodes.Count; i++)
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
				var match = templateNode.Attributes["match"] != null ? templateNode.Attributes["match"].Value : "";
				var name = templateNode.Attributes["name"] != null ? templateNode.Attributes["name"].Value : "";
				var mode = templateNode.Attributes["mode"] != null ? templateNode.Attributes["mode"].Value : "";

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
			var importedFileIds = (from f in context.FilesRelations.Local
										  where f.Target.Id == fileId && f.Mode == FilesRelationMode.Import
										  orderby f.Order
										  select f.Source.Id
										 ).ToList();

			var includedFileIds = (from f in context.FilesRelations.Local
										  where f.Target.Id == fileId && f.Mode == FilesRelationMode.Include
										  orderby f.Order
										  select f.Source.Id).ToList();

			var importTemplates = new List<Template>();
			foreach (var file in importedFileIds)
			{
				if (!ProcessedFiles.Contains(file))
				{
					ProcessTemplates(context, file);
				}

				if (TemplateScope.ContainsKey(file))
				{
					foreach (var template in TemplateScope[file])
					{
						var elem = importTemplates.FirstOrDefault(t => t.Selector == template.Selector &&
							t.SelectorValue == template.SelectorValue && t.Mode == template.Mode);

						if (elem != null)
						{
							var index = importTemplates.IndexOf(elem);
							importTemplates[index] = template;
						}
						else
						{
							importTemplates.Add(template);
						}
					}
				}
			}

			var includeTemplates = new List<Template>();
			foreach (var file in includedFileIds)
			{
				if (!ProcessedFiles.Contains(file))
				{
					ProcessTemplates(context, file);
				}

				if (TemplateScope.ContainsKey(file))
				{
					foreach (var template in TemplateScope[file])
					{
						var elem = includeTemplates.FirstOrDefault(t => t.Selector == template.Selector &&
							t.SelectorValue == template.SelectorValue && t.Mode == template.Mode);

						if (elem != null)
						{
							var index = includeTemplates.IndexOf(elem);
							includeTemplates[index] = template;
						}
						else
						{
							includeTemplates.Add(template);
						}
					}
				}
			}

			var fileTemplates = (from t in context.Templates.Local
										where t.File.Id == fileId
										select t).ToList();

			foreach (var template in fileTemplates)
			{
				var elem = includeTemplates.FirstOrDefault(t => t.Selector == template.Selector &&
					t.SelectorValue == template.SelectorValue && t.Mode == template.Mode);

				if (elem != null)
				{
					var index = includeTemplates.IndexOf(elem);
					includeTemplates[index] = template;
				}
				else
				{
					includeTemplates.Add(template);
				}
			}

			if (importTemplates.Count > 0)
			{
				TemplateScope[fileId] = importTemplates;
			}
			else {
				TemplateScope[fileId] = new List<Template>();
			}

			if (includeTemplates.Count > 0)
			{
				foreach (var includeTemplate in includeTemplates)
				{
					var elem = TemplateScope[fileId].FirstOrDefault(t => t.Selector == includeTemplate.Selector &&
					t.SelectorValue == includeTemplate.SelectorValue && t.Mode == includeTemplate.Mode);
					if (elem != null)
					{
						var index = TemplateScope[fileId].IndexOf(elem);
						TemplateScope[fileId][index] = includeTemplate;
					}
					else
					{
						TemplateScope[fileId].Add(includeTemplate);
					}
				}
			}

			Stack<Template> templateStack = new Stack<Template>();
			List<Template> templateList = new List<Template>();


			// Find the template for root node
			var rootTemplate = TemplateScope[fileId].LastOrDefault(t => t.Selector == TemplateSelector.Match && t.SelectorValue == "/" && t.Mode.Length == 0);

			if (rootTemplate != null)
			{
				templateStack.Push(rootTemplate);
				templateList.Add(rootTemplate);
			}

			while (templateStack.Count > 0)
			{
				var current = templateStack.Pop();
				var calls = (from call in context.TemplateCalls
								  where call.Template.Id == current.Id
								  select call).ToList();
				foreach (var call in calls)
				{
					if (call.Selector == TemplateSelector.Name)
					{
						var targetTemplate = TemplateScope[fileId].Where(t => t.Selector == TemplateSelector.Name 
							&& t.SelectorValue == call.SelectorValue && call.Mode == t.Mode && !templateList.Contains(t)).FirstOrDefault();

						if (targetTemplate != null)
						{
							context.TemplatesRelation.Add(new TemplatesRelation { Target = current, Source = targetTemplate });
							templateStack.Push(targetTemplate);
							templateList.Add(targetTemplate);
						}
					}
					else
					{
						IEnumerable<Template> targetTemplates;
						if (call.ForImports)
						{
							targetTemplates = importTemplates.Where(t => !templateList.Contains(t) && t.Selector == TemplateSelector.Match && t.Mode == call.Mode 
							&& (call.SelectorValue == t.SelectorValue || call.SelectorValue.Length == 0));
						}
						else
						{
							targetTemplates = TemplateScope[fileId].Where(t => !templateList.Contains(t) && t.Selector == TemplateSelector.Match && t.Mode == call.Mode 
							&& (call.SelectorValue == t.SelectorValue || call.SelectorValue.Length == 0));
						}

						if (targetTemplates != null)
						{
							foreach (var targetTemplate in targetTemplates)
							{
								context.TemplatesRelation.Add(new TemplatesRelation { Target = current, Source = targetTemplate });
								templateStack.Push(targetTemplate);
								templateList.Add(targetTemplate);
							}
						}
					}
				}
				context.SaveChanges();
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
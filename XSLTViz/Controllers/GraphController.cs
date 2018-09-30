using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using XSLTViz.APIModels;
using XSLTViz.DataModel;

namespace XSLTViz.Controllers
{
    public class GraphController : ApiController
    {
        public Graph Get(int id)
        {
            var nodes = new List<Node>();
            var links = new List<Link>();
            using (var context = new DataContext())
            {
                var files = (from f in context.Files
                            where f.Project.Id == id
                            select f).ToList();

                foreach (File file in files)
                {
                    var node = new Node (file);

                    var fileLinks = (from r in context.FilesRelations
                                    where r.Source.Id == file.Id
                                    select r).ToList();

                    if (fileLinks.Count == 0)
                    {
                        node.IsLeaf = true;
                    }
                    else
                    {
                        foreach (FilesRelation link in fileLinks)
                        {
                            links.Add(new Link
                            {
                                Source = link.Source.Id - 1,
                                Target = link.Target.Id - 1
                            });
                        }
                    }

                    nodes.Add(node);
                }
            }
            return new Graph
            {
                Nodes = nodes,
                Links = links
            };
        }

        [Route("api/graph/{fileId}/path")]
        [HttpGet]
        public Graph GetPath(int fileId)
        {
            var nodes = new List<Node>();
            var links = new List<Link>();

            var stack = new Stack<Node>();
            
            using (var context = new DataContext())
            {
                var file = (from f in context.Files
                             where f.Id == fileId
                             select f).FirstOrDefault();

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

                        links.Add(new Link { Source = node.Id, Target = current.Id });
                    }
                }
            }

            return new Graph
            {
                Nodes = nodes,
                Links = links
            };
        }

		[Route("api/graph/{projectId}/trees")]
		[HttpGet]
		public List<TreeNode> GetTrees(int projectId)
		{
			var nodes = new List<TreeNode>();

			using (var context = new DataContext())
			{
				var files = (from f in context.Files
								 where f.Project.Id == projectId
								 select f).ToList();



				foreach (var file in files)
				{
					var node = new TreeNode(file);

					var imports = (from f in context.FilesRelations
										where f.Target.Id == file.Id
										select f).ToList();

					List<string> importNodes = new List<string>();
					foreach (var importFile in imports)
					{
						importNodes.Add(importFile.Source.Path);
					}

					node.Imports = importNodes;
					nodes.Add(node);
				}

			}
			return nodes;
		}

		//[Route("api/graph/treeview/{fileId}")]
		//[HttpGet]
		//public Graph GetTreeView(int fileId)
		//{
		//	var nodes = new List<Node>();
		//	var links = new List<Link>();
		//}
    }
}

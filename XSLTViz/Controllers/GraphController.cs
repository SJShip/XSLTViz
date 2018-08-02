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
        public FilesRelationGraph Get(int id)
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
                    nodes.Add(new Node { Id = file.Id, Name = file.Path });
                    var fileLinks = (from r in context.FilesRelations
                                    where r.Source.Id == file.Id
                                    select r).ToList();

                    foreach (FilesRelation link in fileLinks)
                    {
                        links.Add(new Link
                        {
                            Source = link.Source.Id - 1,
                            Target = link.Target.Id - 1
                        });
                    }

                }
            }
            return new FilesRelationGraph
            {
                Nodes = nodes,
                Links = links
            };
        }
    }
}

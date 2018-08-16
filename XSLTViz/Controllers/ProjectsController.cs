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
    public class ProjectsController : ApiController
    {
        public ProjectInfo Get(int id)
        {
            using (var context = new DataContext())
            {
                var project = (from p in context.Projects
                               where p.Id == id
                               select p).First();

                var totalFiles = (from f in context.Files
                               where f.Project.Id == id
                               select f).Count();


                return new ProjectInfo(project, totalFiles);
            }
        }

        public HttpResponseMessage Patch(int id, [FromBody] Settings settings)
        {
            using (var context = new DataContext())
            {
                var project = (from p in context.Projects
                            where p.Id == id
                               select p).First();

                if (project == null)
                {
                    return Request.CreateResponse(System.Net.HttpStatusCode.NotFound);
                }

                project.Settings = settings;             

                context.SaveChanges();
                var response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
                return response;
            }
        }
    }
}

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
    public class FilesController : ApiController
    {
        public HttpResponseMessage Post(int id, Node node)
        {
            using (var context = new DataContext())
            {
                var file = (from f in context.Files
                             where f.Id == id
                             select f).First();

                if (file == null)
                {
                    return Request.CreateResponse(System.Net.HttpStatusCode.NotFound);
                }

                file.IsFixed = node.IsFixed;

                if (node.IsFixed)
                {
                    file.Point.X = node.X;
                    file.Point.Y = node.Y;
                }

                context.SaveChanges();
                var response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
                return response;
            }
        }
    }
}

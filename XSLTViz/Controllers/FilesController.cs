using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
        public HttpResponseMessage Patch([FromBody] Node node)
        {
            using (var context = new DataContext())
            {
                var file = (from f in context.Files
                            where f.Id == node.Id
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


        [Route("api/files/{fileId}/content")]
        [HttpGet]
        public string GetFileContent(int fileId)
        {
            using (var context = new DataContext())
            {
                var fileContent = (from f in context.Files
                            where f.Id == fileId
                            select f.Content).First();

                return fileContent;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XSLTViz.DataModel;

namespace XSLTViz
{
    public class HomeController: Controller
    {
        public ActionResult Index()
        {
            var dbContext = new DataContext();
            dbContext.SaveChanges();

            return View();
        }
    }
}
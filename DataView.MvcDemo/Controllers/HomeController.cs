using DuncanApps.DataView;
using DuncanApps.DataView.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DataView.MvcDemo.Controllers
{
    public class HomeController : Controller
    {
        string[] colors = new[] { "red", "pink", "purple", "deep-purple", "indigo", "blue", "light-blue", "cyan", "teal", "green", "light-green", "lime", "yellow", "amber", "orange", "deep-orange", "brown", "grey", "blue-grey" };

        public ActionResult Index(DataViewRequest request)
        {
            return Json(colors.AsQueryable().ToDataView(request), JsonRequestBehavior.AllowGet);
        }
    }
}
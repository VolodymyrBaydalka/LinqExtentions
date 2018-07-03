using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DuncanApps.DataView;
using DuncanApps.DataView.Mvc;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DataView.MvcDemoCore.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        string[] colors = new[] {"red", "pink", "purple", "deep-purple", "indigo", "blue", "light-blue", "cyan", "teal", "green", "light-green", "lime", "yellow", "amber", "orange", "deep-orange", "brown", "grey", "blue-grey"};
        // GET api/values
        [HttpGet]
        public DataView<string> Get([ModelBinder(binderType:typeof(DataViewRequestBinder))] DataViewRequest request)
        {
            return colors.AsQueryable().ToDataView(request);
        }
    }
}

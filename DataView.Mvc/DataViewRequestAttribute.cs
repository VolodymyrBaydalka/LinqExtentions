using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace DuncanApps.DataView.Mvc
{
    public class DataViewRequestAttribute : CustomModelBinderAttribute
    {
        public string Prefix { get; set; } = "$";

        public override IModelBinder GetBinder()
        {
            return new DataViewRequestBinder();
        }
    }
}

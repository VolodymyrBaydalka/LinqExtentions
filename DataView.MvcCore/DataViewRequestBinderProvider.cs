using DuncanApps.DataView.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace DuncanApps.DataView.MvcCore
{
    public class DataViewRequestBinderProvider : IModelBinderProvider
    {
        public string Prefix { get; set; } = "$";

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(DataViewRequest))
            {
                return new DataViewRequestBinder { Prefix = this.Prefix };
            }

            return null;
        }
    }
}

using DuncanApps.DataView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;

namespace DuncanApps.DataView.Mvc
{
    public class DataViewRequestBinder : IModelBinder
    {
        public string Prefix { get; set; }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var result = new DataViewRequest();

            var values = bindingContext.ValueProvider;
            var orderText = values.GetValue($"{Prefix}orderby")?.AttemptedValue;
            var whereText = values.GetValue($"{Prefix}where")?.AttemptedValue;
            var skipText = values.GetValue($"{Prefix}skip")?.AttemptedValue;
            var takeText = values.GetValue($"{Prefix}take")?.AttemptedValue;

            result.Skip = skipText == null ? 0 : Convert.ToInt32(skipText);
            result.Take = takeText == null ? 0 : Convert.ToInt32(takeText);
            result.OrderBy = orderText == null ? null : ParseHelper.PasreOrderClause(orderText);
            result.Where = whereText == null ? null : ParseHelper.PasreWhereClause(orderText);

            return result;
        }
    }
}

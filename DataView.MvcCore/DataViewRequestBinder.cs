using DuncanApps.DataView.Converters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;

namespace DuncanApps.DataView.Mvc
{
    public class DataViewRequestBinder : IModelBinder
    {
        public string Prefix { get; set; } = "$";

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var result = new DataViewRequest();

            var values = bindingContext.ValueProvider;
            var orderText = values.GetValue($"{Prefix}orderby").FirstValue;
            var whereText = values.GetValue($"{Prefix}where").FirstValue;
            var skipText = values.GetValue($"{Prefix}skip").FirstValue;
            var takeText = values.GetValue($"{Prefix}take").FirstValue;

            result.Skip = skipText == null ? 0 : Convert.ToInt32(skipText);
            result.Take = takeText == null ? 0 : Convert.ToInt32(takeText);
            result.OrderBy = orderText == null ? null : ParseHelper.PasreOrderClause(orderText);
            result.Where = whereText == null ? null : ParseHelper.PasreWhereClause(whereText);

            bindingContext.Result = ModelBindingResult.Success(result);

            return Task.CompletedTask;
        }
    }
}

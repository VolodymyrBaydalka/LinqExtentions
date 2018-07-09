using DuncanApps.DataView.Converters;
using System.Collections.Generic;
using System.ComponentModel;

namespace DuncanApps.DataView
{
    public class DataViewRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }

        public IWhereClause Where { get; set; }
        [TypeConverter(typeof(OrderClausesConverter))]
        public IList<OrderClause> OrderBy { get; set; }
    }
}

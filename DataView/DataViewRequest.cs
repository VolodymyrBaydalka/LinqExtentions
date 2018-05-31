using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuncanApps.DataView
{
    public class DataViewRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public IWhereClause Where { get; set; }
        public IList<OrderClause> OrderBy { get; set; }
    }
}

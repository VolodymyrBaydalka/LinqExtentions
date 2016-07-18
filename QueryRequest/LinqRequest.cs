using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZV.LinqExtentions
{
    public class LinqRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public IWhereClause Where { get; set; }
        public IList<OrderClause> OrderBy { get; set; }
    }
}

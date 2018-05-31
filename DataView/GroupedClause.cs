using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DuncanApps.DataView
{
    public class GroupedClause : IWhereClause
    {
        public WhereLogic Logic { get; set; }
        public IList<IWhereClause> SubClauses { get; set; }
    }
}

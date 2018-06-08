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

        public override string ToString()
        {
            var res = new StringBuilder("(");

            for (int i = 0, len = SubClauses?.Count ?? 0; i < len; i++)
            {
                if (i != 0)
                    res.Append(") ").Append(this.Logic).Append(" (");

                res.Append(SubClauses[i]);
            }

            return res.Append(")").ToString();
        }
    }
}

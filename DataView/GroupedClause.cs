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

        public override bool Equals(object obj)
        {
            return obj is GroupedClause clause && Logic == clause.Logic &&
                   EqualityComparer<IList<IWhereClause>>.Default.Equals(SubClauses, clause.SubClauses);
        }

        public override int GetHashCode()
        {
            var hashCode = -1955899414;
            hashCode = hashCode * -1521134295 + Logic.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IList<IWhereClause>>.Default.GetHashCode(SubClauses);
            return hashCode;
        }
    }
}

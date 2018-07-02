using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DuncanApps.DataView
{
    public class WhereClause : IWhereClause
    {
        public string Field { get; set; }
        public WhereOperator Operator { get; set; }
        public object Value { get; set; }

        public WhereClause()
        {
        }

        public WhereClause(string field, WhereOperator op, object value)
        {
            this.Field = field;
            this.Operator = op;
            this.Value = value;
        }

        public override string ToString()
        {
            return $"{Field} {Operator} {Value}";
        }

        public override bool Equals(object obj)
        {
            return obj is WhereClause clause && Field == clause.Field && Operator == clause.Operator 
                && EqualityComparer<object>.Default.Equals(Value, clause.Value);
        }

        public override int GetHashCode()
        {
            var hashCode = -1117517818;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Field);
            hashCode = hashCode * -1521134295 + Operator.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Value);
            return hashCode;
        }
    }
}

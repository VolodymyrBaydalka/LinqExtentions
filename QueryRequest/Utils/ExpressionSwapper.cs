using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ZV.LinqExtentions
{
    internal class ExpressionSwapper : ExpressionVisitor
    {
        public Dictionary<Expression, Expression> Swaps = new Dictionary<Expression, Expression>();

        public override Expression Visit(Expression node)
        {
            Expression result = null;
            return this.Swaps.TryGetValue(node, out result) ? result : base.Visit(node);
        }
    }
}

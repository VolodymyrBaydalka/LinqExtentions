using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ZV.LinqExtentions
{
    public static class LinqRequestExtentions
    {
        #region Helper methods
        public static LinqRequest Skip(this LinqRequest @this, int count)
        {
            @this.Skip = count;
            return @this;
        }

        public static LinqRequest Take(this LinqRequest @this, int take)
        {
            @this.Take = take;
            return @this;
        }

        public static LinqRequest OrderBy(this LinqRequest @this, string field, ListSortDirection direction = ListSortDirection.Ascending)
        {
            if (@this.OrderBy == null)
                @this.OrderBy = new List<OrderClause>();

            @this.OrderBy.Add(new OrderClause { Field = field, Direction = direction });
            return @this;
        }

        public static LinqRequest Where(this LinqRequest @this, string field, WhereOperator op, object value)
        {
            @this.Where = new WhereClause { Field = field, Operator = op, Value = value };
            return @this;
        }

        public static LinqRequest Where(this LinqRequest @this, IWhereClause where)
        {
            @this.Where = where;
            return @this;
        }

        internal static GroupedClause GroupClause(this IWhereClause left, WhereLogic logic, IWhereClause right)
        {
            var grouped = left as GroupedClause;

            if (grouped == null || grouped.Logic != logic)
            {
                grouped = new GroupedClause { Logic = logic, SubClauses = new List<IWhereClause> { left } };
            }

            grouped.SubClauses.Add(right);

            return grouped;
        }

        public static GroupedClause Or(this IWhereClause left, IWhereClause right)
        {
            return GroupClause(left, WhereLogic.Or, right);
        }

        public static GroupedClause And(this IWhereClause left, IWhereClause right)
        {
            return GroupClause(left, WhereLogic.And, right);
        }

        public static GroupedClause Or(this IWhereClause left, string field, WhereOperator op, object value)
        {
            return GroupClause(left, WhereLogic.Or, new WhereClause(field, op, value));
        }

        public static GroupedClause And(this IWhereClause left, string field, WhereOperator op, object value)
        {
            return GroupClause(left, WhereLogic.And, new WhereClause(field, op, value));
        }
        #endregion

        public static ListSubset<T> GetSubset<T>(this IQueryable<T> @this, LinqRequest request, RequestResolver resolver = null)
        {
            return (resolver ?? RequestResolver.Default).Resolve(@this, request);
        }

        public static Expression ValueExpression(this WhereClause where, Type type)
        {
            var convertType = Nullable.GetUnderlyingType(type) ?? type;
            return Expression.Constant(Convert.ChangeType(where.Value, convertType), type);
        }

        public static Expression BuildExpression(this WhereOperator op, Expression left, Expression right)
        {
            switch (op)
            {
                case WhereOperator.IsEqualTo:
                    return Expression.Equal(left, right);
                case WhereOperator.IsNotEqualTo:
                    return Expression.NotEqual(left, right);
                case WhereOperator.IsLessThan:
                    return Expression.LessThan(left, right);
                case WhereOperator.IsLessThanOrEqualTo:
                    return Expression.LessThanOrEqual(left, right);
                case WhereOperator.IsGreaterThanOrEqualTo:
                    return Expression.GreaterThanOrEqual(left, right);
                case WhereOperator.IsGreaterThan:
                    return Expression.GreaterThan(left, right);
                case WhereOperator.StartsWith:
                    return Expression.Call(left, nameof(string.StartsWith), null, right);
                case WhereOperator.EndsWith:
                    return Expression.Call(left, nameof(string.EndsWith), null, right);
                case WhereOperator.Contains:
                    return Expression.Call(left, nameof(string.Contains), null, right);
            }

            throw new NotSupportedException();
        }
    }
}

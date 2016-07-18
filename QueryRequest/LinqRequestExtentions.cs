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
        public static LinqRequest Skip(this LinqRequest @this, int count) {
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

        public static ListSubset<T> Subset<T>(this IQueryable<T> @this, LinqRequest request)
        {
            IQueryable q = @this;
            var result = new ListSubset<T> {
                Skipped = request.Skip,
                Taken = request.Take,
                Items = new List<T>()
            };

            if (request.Where != null) {
                q = q.Where(request.Where);

                result.Total = q.Provider.Execute<int>(Expression.Call(typeof(Queryable), nameof(Queryable.Count), new[] { q.ElementType }, q.Expression));
            }

            var orderBy = request.OrderBy ?? new List<OrderClause>(1);

            if (orderBy.Count == 0)
            {
                orderBy.Add(DefaultOrderClause(q.ElementType));
            }

            q = q.OrderBy(orderBy);
            q = q.SkipTake(request.Skip, request.Take);

            foreach (var item in q)
            {
                result.Items.Add((T)item);
            }

            if (result.Total < result.Items.Count)
                result.Total = result.Items.Count;

            return result;
        }

        internal static OrderClause DefaultOrderClause(Type type)
        {
            string propertyName = null;

            foreach (var prop in type.GetProperties())
            {
                if (!prop.PropertyType.GetInterfaces().Contains(typeof(IConvertible)))
                    continue;

                if (propertyName == null || prop.IsDefined(typeof(KeyAttribute), false))
                { 
                    propertyName = prop.Name;
                }
            }

            return new OrderClause { Field = propertyName };
        }

        internal static IQueryable OrderBy(this IQueryable q, IEnumerable<OrderClause> clauses)
        {
            var ordered = false;
            var param = Expression.Parameter(q.ElementType, "x");

            foreach (var clause in clauses)
            {
                var methodName = clause.Direction == ListSortDirection.Ascending
                    ? (ordered ? nameof(Queryable.ThenBy) : nameof(Queryable.OrderBy))
                    : (ordered ? nameof(Queryable.ThenByDescending) : nameof(Queryable.OrderByDescending));

                var lambda = Expression.Lambda(Expression.PropertyOrField(param, clause.Field), param);
                q = q.Provider.CreateQuery(Expression.Call(typeof(Queryable), methodName, new[] { q.ElementType, lambda.Body.Type }, q.Expression, Expression.Quote(lambda)));

                ordered = true;
            }

            return q;
        }

        internal static IQueryable SkipTake(this IQueryable q, int skip, int take)
        {
            if (skip != 0)
                q = q.Provider.CreateQuery(Expression.Call(typeof(Queryable), nameof(Queryable.Skip), new[] { q.ElementType }, q.Expression, Expression.Constant(skip)));

            if (take != 0)
                q = q.Provider.CreateQuery(Expression.Call(typeof(Queryable), nameof(Queryable.Take), new[] { q.ElementType }, q.Expression, Expression.Constant(take)));

            return q;
        }

        internal static IQueryable Where(this IQueryable q, IWhereClause where)
        {
            var param = Expression.Parameter(q.ElementType, "x");
            var expr = Expression.Lambda(BuildExpression(param, where), param);

            return q.Provider.CreateQuery(Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { q.ElementType }, q.Expression, expr));
        }

        internal static Expression BuildExpression(ParameterExpression param, GroupedClause clause)
        {
            Expression result = null;

            foreach (var w in clause.SubClauses)
            {
                var exp = BuildExpression(param, w);

                result = result == null ? exp : (clause.Logic == WhereLogic.And ? Expression.AndAlso(result, exp) : Expression.OrElse(result, exp));
            }

            return result;
        }

        internal static Expression BuildExpression(ParameterExpression param, IWhereClause clause)
        {
            var where = clause as WhereClause;
            var grouped = clause as GroupedClause;

            if (where != null)
                return BuildExpression(param, where);
            if (grouped != null)
                return BuildExpression(param, grouped);

            throw new NotSupportedException("clause");
        }

        internal static Expression BuildExpression(ParameterExpression param, WhereClause where)
        {
            var left = Expression.PropertyOrField(param, where.Field);
            var right = Expression.Constant(Convert.ChangeType(where.Value, left.Type), left.Type);

            switch (where.Operator)
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace DuncanApps.DataView
{
    public class RequestResolver
    {
        #region Members
        private Dictionary<Type, Func<object, object>> _converters = new Dictionary<Type, Func<object, object>>();
        #endregion

        #region Properties
        public static RequestResolver Default { get; set; } = new RequestResolver();
        #endregion

        #region Implementation
        public void RegisterConverter<T>(Func<object, T> converter)
        {
            _converters.Add(typeof(T), x => converter(x));
        }

        public void RegisterConverter(Type type, Func<object, object> converter)
        {
            _converters.Add(type, converter);
        }

        public virtual DataView<T> Resolve<T>(IQueryable<T> queryable, DataViewRequest request)
        {
            IQueryable q = queryable;
            var result = new DataView<T>
            {
                Skipped = request.Skip,
                Taken = request.Take,
                Items = new List<T>()
            };

            if (request.Where != null)
                q = ResolveWhere(q, request.Where);

            result.Total = q.Provider.Execute<int>(Expression.Call(typeof(Queryable), nameof(Queryable.Count), new[] { q.ElementType }, q.Expression));

            var orderBy = request.OrderBy ?? new List<OrderClause>(1);

            if (orderBy.Count == 0)
            {
                orderBy.Add(DefaultOrderClause(q.ElementType));
            }

            q = ResolveOrderBy(q, orderBy);
            q = ResolveSkipTake(q, request.Skip, request.Take);

            foreach (var item in q)
            {
                result.Items.Add((T)item);
            }

            return result;
        }

        protected virtual OrderClause DefaultOrderClause(Type type)
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

        protected virtual IQueryable ResolveOrderBy(IQueryable q, IEnumerable<OrderClause> clauses)
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

        protected virtual IQueryable ResolveSkipTake(IQueryable q, int skip, int take)
        {
            if (skip != 0)
                q = q.Provider.CreateQuery(Expression.Call(typeof(Queryable), nameof(Queryable.Skip), new[] { q.ElementType }, q.Expression, Expression.Constant(skip)));

            if (take != 0)
                q = q.Provider.CreateQuery(Expression.Call(typeof(Queryable), nameof(Queryable.Take), new[] { q.ElementType }, q.Expression, Expression.Constant(take)));

            return q;
        }

        protected virtual IQueryable ResolveWhere(IQueryable q, IWhereClause where)
        {
            var param = Expression.Parameter(q.ElementType, "x");
            var expr = Expression.Lambda(BuildExpression(param, where), param);

            return q.Provider.CreateQuery(Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { q.ElementType }, q.Expression, expr));
        }

        protected virtual Expression BuildExpression(ParameterExpression param, GroupedClause clause)
        {
            Expression result = null;

            foreach (var w in clause.SubClauses)
            {
                var exp = BuildExpression(param, w);

                result = result == null ? exp : (clause.Logic == WhereLogic.And ? Expression.AndAlso(result, exp) : Expression.OrElse(result, exp));
            }

            return result;
        }

        protected virtual Expression BuildExpression(ParameterExpression param, IWhereClause clause)
        {
            if (clause is WhereClause where)
                return BuildExpression(param, where);
            if (clause is GroupedClause grouped)
                return BuildExpression(param, grouped);

            throw new NotSupportedException("clause");
        }

        protected virtual Expression BuildExpression(ParameterExpression param, WhereClause where)
        {
            var left = Expression.PropertyOrField(param, where.Field);
            var right = Expression.Constant(GetValue(left, where), left.Type);

            return where.Operator.BuildExpression(left, right);
        }

        protected virtual object GetValue(MemberExpression left, WhereClause where)
        {
            if (_converters.TryGetValue(left.Type, out var convert))
                return convert(where.Value);

            var convertType = Nullable.GetUnderlyingType(left.Type) ?? left.Type;
            return Convert.ChangeType(where.Value, convertType);
        }

        public static Expression FromLambda<T1, T2>(Expression<Func<T1, T2>> expr, Expression param)
        {
            var swaper = new ExpressionSwapper
            {
                Swaps = new Dictionary<Expression, Expression> { { expr.Parameters[0], param } }
            };

            return swaper.Visit(expr.Body);
        }
        #endregion
    }
}

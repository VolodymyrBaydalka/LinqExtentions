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
    public class RequestResolver
    {
        #region Members
        private Dictionary<Type, Func<Expression, WhereClause, Expression>> whereResolvers = new Dictionary<Type, Func<Expression, WhereClause, Expression>>();
        #endregion

        #region Properties
        public static RequestResolver Default { get; private set; } = new RequestResolver();
        #endregion

        #region Implementation
        public void RegisterWhereResolver<T>(Func<Expression, WhereClause, Expression> resolver)
        {
            if (resolver == null)
                whereResolvers.Remove(typeof(T));
            else
                whereResolvers[typeof(T)] = resolver;
        }
        public ListSubset<T> Resolve<T>(IQueryable<T> queryable, LinqRequest request)
        {
            IQueryable q = queryable;
            var result = new ListSubset<T>
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

        internal OrderClause DefaultOrderClause(Type type)
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

        internal static IQueryable ResolveOrderBy(IQueryable q, IEnumerable<OrderClause> clauses)
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

        internal IQueryable ResolveSkipTake(IQueryable q, int skip, int take)
        {
            if (skip != 0)
                q = q.Provider.CreateQuery(Expression.Call(typeof(Queryable), nameof(Queryable.Skip), new[] { q.ElementType }, q.Expression, Expression.Constant(skip)));

            if (take != 0)
                q = q.Provider.CreateQuery(Expression.Call(typeof(Queryable), nameof(Queryable.Take), new[] { q.ElementType }, q.Expression, Expression.Constant(take)));

            return q;
        }

        internal IQueryable ResolveWhere(IQueryable q, IWhereClause where)
        {
            var param = Expression.Parameter(q.ElementType, "x");
            var expr = Expression.Lambda(BuildExpression(param, where), param);

            return q.Provider.CreateQuery(Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { q.ElementType }, q.Expression, expr));
        }

        internal Expression BuildExpression(ParameterExpression param, GroupedClause clause)
        {
            Expression result = null;

            foreach (var w in clause.SubClauses)
            {
                var exp = BuildExpression(param, w);

                result = result == null ? exp : (clause.Logic == WhereLogic.And ? Expression.AndAlso(result, exp) : Expression.OrElse(result, exp));
            }

            return result;
        }

        internal Expression BuildExpression(ParameterExpression param, IWhereClause clause)
        {
            var where = clause as WhereClause;
            var grouped = clause as GroupedClause;

            if (where != null)
                return BuildExpression(param, where);
            if (grouped != null)
                return BuildExpression(param, grouped);

            throw new NotSupportedException("clause");
        }

        internal Expression BuildExpression(ParameterExpression param, WhereClause where)
        {
            Func<Expression, WhereClause, Expression> customResolver = null;

            if (whereResolvers.TryGetValue(param.Type, out customResolver))
            {
                var custom = customResolver(param, where);

                if (custom != null)
                    return custom;
            }

            var left = Expression.PropertyOrField(param, where.Field);
            var right = where.ValueExpression(left.Type);

            return where.Operator.BuildExpression(left, right);
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

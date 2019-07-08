using System;
using System.Collections;
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
        /// <summary>
        /// Default data view request resolver
        /// </summary>
        public static RequestResolver Default { get; set; } = new RequestResolver();

        /// <summary>
        /// IIndicates keyword to sorting or filtering by item itself.
        /// </summary>
        public string ItemKeyword { get; set; } = "@item";

        /// <summary>
        /// Indicates whether default sorting will be applied (by first property of simple type by default).
        /// Entity framework requires ordered queryable to apply Skip or Take methods
        /// </summary>
        public bool ApplyDefaultOrdering { get; set; } = true;

        /// <summary>
        /// Indicates whether resolver should detect collections and aplly filters to collection items.
        /// </summary>
        public bool HandleCollection { get; set; } = true;

        /// <summary>
        /// Indicates whether resolver should detect collections and aplly filters to collection items.
        /// </summary>
        public bool ParseEnums { get; set; } = false;
        #endregion

        #region Implementation
        public void RegisterValueConverter<T>(Func<object, T> converter)
        {
            _converters.Add(typeof(T), x => converter(x));
        }

        public void RegisterValueConverter(Type type, Func<object, object> converter)
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

            if (orderBy.Count == 0 && ApplyDefaultOrdering)
            {
                var clause = DefaultOrderClause(q.ElementType);

                if (clause != null)
                    orderBy.Add(clause);
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

            if (IsSimpleType(type))
                return null;

            foreach (var prop in type.GetProperties())
            {
                if (!prop.PropertyType.GetInterfaces().Contains(typeof(IConvertible)))
                    continue;

                if (propertyName == null || prop.IsDefined(typeof(KeyAttribute), false))
                    propertyName = prop.Name;
            }

            return new OrderClause { Field = propertyName };
        }

        protected virtual IQueryable ResolveOrderBy(IQueryable q, IEnumerable<OrderClause> clauses)
        {
            var isFirstClause = true;
            var param = Expression.Parameter(q.ElementType, "x");

            foreach (var clause in clauses)
            {
                q = ResolveOrderBy(q, clause, isFirstClause);
                isFirstClause = false;
            }

            return q;
        }

        protected virtual IQueryable ResolveOrderBy(IQueryable q, OrderClause clause, bool isFirstClause)
        {
            var methodName = clause.Direction == ListSortDirection.Ascending
                ? (isFirstClause ? nameof(Queryable.OrderBy) : nameof(Queryable.ThenBy))
                : (isFirstClause ? nameof(Queryable.OrderByDescending) : nameof(Queryable.ThenByDescending));

            var lambda = BuildOrderByExpression(Expression.Parameter(q.ElementType, "x"), clause);

            return q.Provider.CreateQuery(Expression.Call(typeof(Queryable), methodName,
                new[] { q.ElementType, lambda.Body.Type }, q.Expression, Expression.Quote(lambda)));
        }

        protected virtual LambdaExpression BuildOrderByExpression(ParameterExpression param, OrderClause clause)
        {
            return clause.Field == ItemKeyword ? Expression.Lambda(param, param)
                : Expression.Lambda(Expression.PropertyOrField(param, clause.Field), param);
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
            var expr = BuildWhereExpression(param, where);

            if (expr == null)
                return q;

            return q.Provider.CreateQuery(Expression.Call(typeof(Queryable), nameof(Queryable.Where),
                new[] { q.ElementType }, q.Expression, Expression.Lambda(expr, param)));
        }

        protected virtual Expression BuildWhereExpression(ParameterExpression param, GroupedClause clause)
        {
            Expression result = null;

            foreach (var w in clause.SubClauses)
            {
                var exp = BuildWhereExpression(param, w);

                if (exp == null)
                    continue;

                result = result == null ? exp : (clause.Logic == WhereLogic.And ? Expression.AndAlso(result, exp) : Expression.OrElse(result, exp));
            }

            return result;
        }

        protected virtual Expression BuildWhereExpression(ParameterExpression param, IWhereClause clause)
        {
            if (clause is WhereClause where)
                return BuildWhereExpression(param, where);
            if (clause is GroupedClause grouped)
                return BuildWhereExpression(param, grouped);

            throw new NotSupportedException("clause");
        }

        protected virtual Expression BuildWhereExpression(ParameterExpression param, WhereClause where)
        {
            var left = GetFieldExpression(param, where.Field);
            var itemType = HandleCollection ? GetCollectionItemType(left.Type) : null;

            if (itemType != null)
            {
                var itemLeft = Expression.Parameter(itemType, "y");
                var itemRight = Expression.Constant(GetValue(itemLeft, where), itemType);
                var itemLambda = Expression.Lambda(BuildOperatorExpression(where.Operator, itemLeft, itemRight), itemLeft);

                return Expression.Call(typeof(Enumerable), nameof(Enumerable.Any), new[] { itemType }, left, itemLambda);
            }

            var right = Expression.Constant(GetValue(left, where), left.Type);
            return BuildOperatorExpression(where.Operator, left, right);
        }

        protected virtual Expression BuildOperatorExpression(WhereOperator @operator, Expression left, Expression right)
        {
            return @operator.BuildExpression(left, right);
        }

        protected virtual Expression GetFieldExpression(ParameterExpression param, string field)
        {
            return field == ItemKeyword ? param : (Expression)Expression.PropertyOrField(param, field);
        }

        protected virtual object GetValue(Expression left, WhereClause where)
        {
            if (_converters.TryGetValue(left.Type, out var convert))
                return convert(where.Value);

            var convertType = Nullable.GetUnderlyingType(left.Type) ?? left.Type;

            if (ParseEnums && convertType.IsEnum && where.Value is string stringValue)
                return Enum.Parse(convertType, stringValue);

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

        protected static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(string);
        }

        protected static Type GetCollectionItemType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            if(typeof(ICollection).IsAssignableFrom(type) && type.IsGenericType)
                return type.GetGenericArguments().Single();

            return null;
        }
        #endregion
    }
}

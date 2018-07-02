using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DuncanApps.DataView.Mvc
{
    public static class ParseHelper
    {
        private static Dictionary<string, WhereOperator> operatorShortNames = new Dictionary<string, WhereOperator>(StringComparer.OrdinalIgnoreCase)
        {
            { "eq", WhereOperator.IsEqualTo },
            { "neq", WhereOperator.IsNotEqualTo },
            { "lt", WhereOperator.IsLessThan },
            { "lte", WhereOperator.IsLessThanOrEqualTo},
            { "gt", WhereOperator.IsGreaterThan },
            { "gte", WhereOperator.IsGreaterThanOrEqualTo },
        };
        private static Dictionary<string, ListSortDirection> directionShortNames = new Dictionary<string, ListSortDirection>(StringComparer.OrdinalIgnoreCase)
        {
            { "asc", ListSortDirection.Ascending },
            { "desc", ListSortDirection.Descending }
        };

        public static IWhereClause PasreWhereClause(string text)
        {
            IWhereClause result = null;

            return result;
        }

        public static IList<OrderClause> PasreOrderClause(string text)
        {
            var parts = text.Split(',');
            var result = new List<OrderClause>();

            foreach (var part in parts)
            {
                var words = part.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var clause = new OrderClause { Field = words[0].Trim() };

                if (words.Length > 1)
                    clause.Direction = ParseListSortDirection(words[1]);

                result.Add(clause);
            }

            return result;
        }

        public static WhereOperator ParseWhereOperator(string text)
        {
            if (operatorShortNames.TryGetValue(text, out var val))
                return val;

            return (WhereOperator)Enum.Parse(typeof(WhereOperator), text, true);
        }

        public static ListSortDirection ParseListSortDirection(string text)
        {
            if (directionShortNames.TryGetValue(text, out var val))
                return val;

            return (ListSortDirection)Enum.Parse(typeof(ListSortDirection), text, true);
        }
    }
}

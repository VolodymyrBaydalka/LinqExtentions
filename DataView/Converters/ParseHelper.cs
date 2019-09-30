using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DuncanApps.DataView.Converters
{
    public static class ParseHelper
    {
        private static Dictionary<string, WhereOperator> operatorShortNames = new Dictionary<string, WhereOperator>(StringComparer.OrdinalIgnoreCase)
        {
            { "eq", WhereOperator.IsEqualTo },
            { "ne", WhereOperator.IsNotEqualTo },
            { "neq", WhereOperator.IsNotEqualTo },
            { "lt", WhereOperator.IsLessThan },
            { "le", WhereOperator.IsLessThanOrEqualTo},
            { "lte", WhereOperator.IsLessThanOrEqualTo},
            { "gt", WhereOperator.IsGreaterThan },
            { "ge", WhereOperator.IsGreaterThanOrEqualTo },
            { "gte", WhereOperator.IsGreaterThanOrEqualTo },
        };
        private static Dictionary<string, ListSortDirection> directionShortNames = new Dictionary<string, ListSortDirection>(StringComparer.OrdinalIgnoreCase)
        {
            { "asc", ListSortDirection.Ascending },
            { "desc", ListSortDirection.Descending }
        };

        static ParseHelper()
        {
        }

        public static IWhereClause PasreWhereClause(string text, bool handleNullValue = false)
        {
            if(string.IsNullOrEmpty(text))
                return null;

            return new FilterParser(text, handleNullValue).Parse();
        }

        public static IList<OrderClause> PasreOrderClause(string text)
        {
            var parts = text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
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

        public static IList<string> PasreList(string text)
        {
            return text
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).ToArray();
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

        public static WhereLogic ParseWhereLogic(string text)
        {
            return (WhereLogic)Enum.Parse(typeof(WhereLogic), text, true);
        }
    }
}

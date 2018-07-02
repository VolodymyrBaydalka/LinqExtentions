using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

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
        private static Regex WhereClauseRegex = new Regex(@"((?<field>[^\s]+)\s+(?<op>[^\s]+)\s+(?<val>[^\s]+))(\s+(?<logic>and|or)\s+)?");

        public static IWhereClause PasreWhereClause(string text)
        {
            IWhereClause result = null;

            var matches = WhereClauseRegex.Matches(text);
            var logicText = "";

            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var logicMatch = match.Groups["logic"];
                var fieldMatch = match.Groups["field"];
                var opMatch = match.Groups["op"];
                var valueMatch = match.Groups["val"];

                var where = new WhereClause
                {
                    Field = fieldMatch.Value,
                    Operator = ParseWhereOperator(opMatch.Value),
                    Value = valueMatch.Value
                };

                if (result == null)
                    result = where;
                else if(!string.IsNullOrEmpty(logicText))
                    result = result.Combine(ParseWhereLogic(logicText), where);

                logicText = logicMatch.Success ? logicMatch.Value : null;
            }

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

        public static WhereLogic ParseWhereLogic(string text)
        {
            return (WhereLogic)Enum.Parse(typeof(WhereLogic), text, true);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private static readonly Regex WhereClauseRegex;

        static ParseHelper()
        {
            WhereClauseRegex = new Regex(@"((?<group>\((?>\((?<depth>)|\)(?<-depth>)|[^()]+)*\)(?(depth)(?!)))|((?<field>[^\s]+)\s+(?<op>[^\s]+)\s+(?<val>[^\s]+)))(\s*(?<logic>and|or)\s*)?");
        }

        public static IWhereClause PasreWhereClause(string text)
        {
            IWhereClause result = null;

            var matches = WhereClauseRegex.Matches(text);
            var logicText = "";

            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var logicMatch = match.Groups["logic"];
                var groupMatch = match.Groups["group"];

                var where = groupMatch.Success
                    ? PasreWhereClause(groupMatch.Value.Substring(1, groupMatch.Length - 2))
                    : new WhereClause
                    {
                        Field = match.Groups["field"].Value,
                        Operator = ParseWhereOperator(match.Groups["op"].Value),
                        Value = match.Groups["val"].Value
                    };

                if (result == null)
                    result = where;
                else
                    result = result.Combine(ParseWhereLogic(logicText), where);

                logicText = logicMatch.Value;
            }

            return result;
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

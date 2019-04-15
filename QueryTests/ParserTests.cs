using System;
using System.ComponentModel;
using DuncanApps.DataView;
using DuncanApps.DataView.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DuncanApps.DataView.Tests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void TestParseWhereLogic()
        {
            Assert.AreEqual(ParseHelper.ParseWhereLogic("and"), WhereLogic.And);
            Assert.AreEqual(ParseHelper.ParseWhereLogic("or"), WhereLogic.Or);
        }

        [TestMethod]
        public void TestParseListSortDirection()
        {
            Assert.AreEqual(ParseHelper.ParseListSortDirection("asc"), ListSortDirection.Ascending);
            Assert.AreEqual(ParseHelper.ParseListSortDirection("desc"), ListSortDirection.Descending);
            Assert.AreEqual(ParseHelper.ParseListSortDirection("ascending"), ListSortDirection.Ascending);
            Assert.AreEqual(ParseHelper.ParseListSortDirection("descending"), ListSortDirection.Descending);
        }

        [TestMethod]
        public void TestParseWhereOperator()
        {
            Assert.AreEqual(ParseHelper.ParseWhereOperator("eq"), WhereOperator.IsEqualTo);
            Assert.AreEqual(ParseHelper.ParseWhereOperator("neq"), WhereOperator.IsNotEqualTo);
            Assert.AreEqual(ParseHelper.ParseWhereOperator("lt"), WhereOperator.IsLessThan);
            Assert.AreEqual(ParseHelper.ParseWhereOperator("lte"), WhereOperator.IsLessThanOrEqualTo);
        }

        [TestMethod]
        public void TestParseOrderClause()
        {
            var empty = ParseHelper.PasreOrderClause(string.Empty);
            Assert.AreEqual(empty.Count, 0);

            var order1 = ParseHelper.PasreOrderClause("field1");

            Assert.AreEqual(order1.Count, 1);
            Assert.AreEqual(order1[0], new OrderClause { Field = "field1" });

            var order2 = ParseHelper.PasreOrderClause("field1 desc");

            Assert.AreEqual(order2.Count, 1);
            Assert.AreEqual(order2[0], new OrderClause { Field = "field1", Direction = ListSortDirection.Descending });

            var order3 = ParseHelper.PasreOrderClause("field1 asc, field2 desc, field3");

            Assert.AreEqual(order3.Count, 3);
            Assert.AreEqual(order3[0], new OrderClause { Field = "field1", Direction = ListSortDirection.Ascending });
            Assert.AreEqual(order3[1], new OrderClause { Field = "field2", Direction = ListSortDirection.Descending });
            Assert.AreEqual(order3[2], new OrderClause { Field = "field3", Direction = ListSortDirection.Ascending });
        }

        [TestMethod]
        public void TestParseWhereClause()
        {
            var nullClause = ParseHelper.PasreWhereClause(null);
            Assert.AreEqual(nullClause, null);

            var empty = ParseHelper.PasreWhereClause(string.Empty);
            Assert.AreEqual(empty, null);

            var where1 = ParseHelper.PasreWhereClause("field1 eq 10");

            Assert.AreEqual(where1, new WhereClause { Field = "field1", Operator = WhereOperator.IsEqualTo, Value = "10" });

            var where2 = ParseHelper.PasreWhereClause("field1 eq 10 and field2 neq 20");

            Assert.AreEqual(where2,
                new WhereClause { Field = "field1", Operator = WhereOperator.IsEqualTo, Value = "10" }
                    .And(new WhereClause { Field = "field2", Operator = WhereOperator.IsNotEqualTo, Value = "20" })
            );

            var where3 = ParseHelper.PasreWhereClause("field1 eq 10 and field2 neq 20 or field3 lt 5");

            Assert.AreEqual(where3,
                new WhereClause { Field = "field1", Operator = WhereOperator.IsEqualTo, Value = "10" }
                    .And(new WhereClause { Field = "field2", Operator = WhereOperator.IsNotEqualTo, Value = "20" })
                    .Or(new WhereClause { Field = "field3", Operator = WhereOperator.IsLessThan, Value = "5" })
            );

            var where4 = ParseHelper.PasreWhereClause("(field1 eq 10 and field2 neq 20) or field3 lt 5");

            Assert.AreEqual(where4,
                new WhereClause { Field = "field1", Operator = WhereOperator.IsEqualTo, Value = "10" }
                    .And(new WhereClause { Field = "field2", Operator = WhereOperator.IsNotEqualTo, Value = "20" })
                    .Or(new WhereClause { Field = "field3", Operator = WhereOperator.IsLessThan, Value = "5" })
            );

            var where5 = ParseHelper.PasreWhereClause("(field1 eq 10 and (field2 neq 20)) or field3 lt 5");

            Assert.AreEqual(where5,
                new WhereClause { Field = "field1", Operator = WhereOperator.IsEqualTo, Value = "10" }
                    .And(new WhereClause { Field = "field2", Operator = WhereOperator.IsNotEqualTo, Value = "20" })
                    .Or(new WhereClause { Field = "field3", Operator = WhereOperator.IsLessThan, Value = "5" })
            );

            var where6 = ParseHelper.PasreWhereClause("(@item ne red)and(@item ne yellow)");

            Assert.AreEqual(where6,
                new WhereClause { Field = "@item", Operator = WhereOperator.IsNotEqualTo, Value = "red" }
                    .And(new WhereClause { Field = "@item", Operator = WhereOperator.IsNotEqualTo, Value = "yellow" })
            );

            var where7 = ParseHelper.PasreWhereClause("field1 eq \"test \\\\\\\"value\"");
            Assert.AreEqual(where7, new WhereClause { Field = "field1", Operator = WhereOperator.IsEqualTo, Value = "test \\\"value" });


            var where8 = ParseHelper.PasreWhereClause(" _field_1 eq \"test \\\"value\" ");
            Assert.AreEqual(where8, new WhereClause { Field = "_field_1", Operator = WhereOperator.IsEqualTo, Value = "test \"value" });

            var where9 = ParseHelper.PasreWhereClause("field1 eq -10.88326");
            Assert.AreEqual(where9, new WhereClause { Field = "field1", Operator = WhereOperator.IsEqualTo, Value = "-10.88326" });

            var where10 = ParseHelper.PasreWhereClause(" _field_1 eq _value");
            Assert.AreEqual(where10, new WhereClause { Field = "_field_1", Operator = WhereOperator.IsEqualTo, Value = "_value" });
        }
    }
}

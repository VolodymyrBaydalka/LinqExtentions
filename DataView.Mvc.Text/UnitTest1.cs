using System;
using System.ComponentModel;
using DuncanApps.DataView;
using DuncanApps.DataView.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataView.Mvc.Text
{
    [TestClass]
    public class UnitTest1
    {
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
    }
}

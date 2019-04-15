using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;

namespace DuncanApps.DataView.Tests
{
    [TestClass]
    public class SimpleTests
    {
        class Item {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public decimal Price { get; set; }
        }

        class CollectionItem
        {
            [Key]
            public int Id { get; set; }
            public string[] Tags { get; set; }
        }

        Item[] testData = new Item[] {
                new Item { Id = 1, Name ="Item 1", Date = new DateTime(2016, 1, 13), Price = 100 },
                new Item { Id = 2, Name ="Item 2", Date = new DateTime(2016, 1, 13), Price = 200 },
                new Item { Id = 3, Name ="Item 3", Date = new DateTime(2016, 1, 13).AddDays(1), Price = 80 },
                new Item { Id = 4, Name ="Item 4", Date = new DateTime(2016, 1, 13).AddDays(1), Price = 90 },
                new Item { Id = 5, Name ="Item 5", Date = new DateTime(2016, 1, 13).AddDays(-1), Price = 100 },
                new Item { Id = 6, Name ="Item 6", Date = new DateTime(2016, 1, 13), Price = 140 },
            };

        [TestMethod]
        public void SkipTakeTest()
        {
            var request = new DataViewRequest {
                Skip = 1,
                Take = 2
            };

            var subset = testData.AsQueryable().ToDataView(request);

            Assert.AreEqual(subset.Total, 6);
            Assert.AreEqual(subset.Skipped, 1);
            Assert.AreEqual(subset.Taken, 2);
            Assert.AreEqual(subset.Items.Count, 2);
            Assert.AreEqual(subset.Items[0].Id, 2);
            Assert.AreEqual(subset.Items[1].Id, 3);
        }

        [TestMethod]
        public void OrderByTest()
        {
            var request = new DataViewRequest
            {
                Take = 2,
                OrderBy = new[] { new OrderClause { Field = nameof(Item.Date) }  }
            };

            var subset = testData.AsQueryable().ToDataView(request);

            Assert.AreEqual(subset.Total, 6);
            Assert.AreEqual(subset.Skipped, 0);
            Assert.AreEqual(subset.Taken, 2);
            Assert.AreEqual(subset.Items.Count, 2);
            Assert.AreEqual(subset.Items[0].Id, 5);
            Assert.AreEqual(subset.Items[1].Id, 1);
        }


        [TestMethod]
        public void WhereTest()
        {
            var request = new DataViewRequest
            {
                Where = new WhereClause(nameof(Item.Price), WhereOperator.IsGreaterThanOrEqualTo, 100),
                OrderBy = new[] { new OrderClause { Field = nameof(Item.Price) } }
            };

            var subset = testData.AsQueryable().ToDataView(request);

            Assert.AreEqual(subset.Total, 4);
            Assert.AreEqual(subset.Skipped, 0);
            Assert.AreEqual(subset.Taken, 0);
            Assert.AreEqual(subset.Items.Count, 4);
            Assert.AreEqual(subset.Items[0].Price, 100);
            Assert.AreEqual(subset.Items[1].Price, 100);
        }

        [TestMethod]
        public void CustomConverterTest()
        {
            var resolver = new RequestResolver();

            resolver.RegisterValueConverter(x =>
            {
                return "null".Equals(x) ? null : Convert.ToString(x);
            });

            var request = new DataViewRequest
            {
                Where = new WhereClause("Name", WhereOperator.IsEqualTo, "null")
            };

            var nullData = new[] {
                new Item { Name = "Item1" },
                new Item { Id = 1, Name = null },
                new Item { Id = 2, Name = null }
            };

            var subset = nullData.AsQueryable().ToDataView(request, resolver);

            Assert.AreEqual(subset.Total, 2);
            Assert.AreEqual(subset.Items.Count, 2);
            Assert.AreEqual(subset.Items[0].Id, 1);
            Assert.AreEqual(subset.Items[1].Id, 2);
        }

        class CustomRequestResolver : RequestResolver
        {
            protected override Expression BuildWhereExpression(ParameterExpression param, WhereClause where)
            {
                if ("date".Equals(where.Field, StringComparison.OrdinalIgnoreCase))
                {
                    var left = FromLambda<Item, int>(i => i.Date.Day, param);
                    return where.Operator.BuildExpression(left, where.ValueExpression(left.Type));
                }

                if ("Name".Equals(where.Field, StringComparison.OrdinalIgnoreCase))
                {
                    return FromLambda<Item, bool>(i => i.Name.EndsWith("4") || i.Name.EndsWith("3"), param);
                }

                return base.BuildWhereExpression(param, where);
            }
        }

        [TestMethod]
        public void CustomResolverTest()
        {
            var resolver = new CustomRequestResolver();
            
            var request = new DataViewRequest
            {
                Where = new WhereClause(nameof(Item.Date), WhereOperator.IsEqualTo, 14).And("Name", WhereOperator.IsEqualTo, "ignored"),
                OrderBy = new[] { new OrderClause { Field = nameof(Item.Price) } }
            };

            var subset = testData.AsQueryable().ToDataView(request, resolver);

            Assert.AreEqual(subset.Total, 2);
            Assert.AreEqual(subset.Skipped, 0);
            Assert.AreEqual(subset.Taken, 0);
            Assert.AreEqual(subset.Items.Count, 2);
            Assert.AreEqual(subset.Items[0].Id, 3);
            Assert.AreEqual(subset.Items[1].Id, 4);
        }

        [TestMethod]
        public void CollectionTest()
        {

            var collectionData = new[] {
                new CollectionItem { Id = 1, Tags = new []{ "a", "b", "c" } },
                new CollectionItem { Id = 2, Tags = new []{ "a", "b", "c", "d" } }
            };

            var request = new DataViewRequest
            {
                Where = new WhereClause(nameof(CollectionItem.Tags), WhereOperator.IsEqualTo, "d"),
            };

            var subset = collectionData.AsQueryable().ToDataView(request);

            Assert.AreEqual(subset.Total, 1);
            Assert.AreEqual(subset.Skipped, 0);
            Assert.AreEqual(subset.Taken, 0);
            Assert.AreEqual(subset.Items[0].Id, 2);
        }
    }
}

using System;
using System.Linq;
using ZV.LinqExtentions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QueryTests
{
    [TestClass]
    public class UnitTest1
    {
        class Item {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public decimal Price { get; set; }
        }

        Item[] testData = new Item[] {
                new Item { Id = 1, Name ="Item 1", Date = DateTime.Now, Price = 100 },
                new Item { Id = 2, Name ="Item 2", Date = DateTime.Now, Price = 200 },
                new Item { Id = 3, Name ="Item 3", Date = DateTime.Now.AddDays(1), Price = 80 },
                new Item { Id = 4, Name ="Item 4", Date = DateTime.Now.AddDays(1), Price = 90 },
                new Item { Id = 5, Name ="Item 5", Date = DateTime.Now.AddDays(-1), Price = 100 },
                new Item { Id = 6, Name ="Item 6", Date = DateTime.Now, Price = 140 },
            };

        [TestMethod]
        public void SkipTakeTest()
        {
            var request = new LinqRequest {
                Skip = 1,
                Take = 2
            };

            var subset = testData.AsQueryable().Subset(request);

            Assert.AreEqual(subset.Total, 2);
            Assert.AreEqual(subset.Skipped, 1);
            Assert.AreEqual(subset.Taken, 2);
            Assert.AreEqual(subset.Items.Count, 2);
            Assert.AreEqual(subset.Items[0].Id, 2);
            Assert.AreEqual(subset.Items[1].Id, 3);
        }

        [TestMethod]
        public void OrderByTest()
        {
            var request = new LinqRequest
            {
                Take = 2,
                OrderBy = new[] { new OrderClause { Field = nameof(Item.Date) }  }
            };

            var subset = testData.AsQueryable().Subset(request);

            Assert.AreEqual(subset.Total, 2);
            Assert.AreEqual(subset.Skipped, 0);
            Assert.AreEqual(subset.Taken, 2);
            Assert.AreEqual(subset.Items.Count, 2);
            Assert.AreEqual(subset.Items[0].Id, 5);
            Assert.AreEqual(subset.Items[1].Id, 1);
        }


        [TestMethod]
        public void WhereTest()
        {
            var request = new LinqRequest
            {
                Where = new WhereClause(nameof(Item.Price), WhereOperator.IsGreaterThanOrEqualTo, 100),
                OrderBy = new[] { new OrderClause { Field = nameof(Item.Price) } }
            };

            var subset = testData.AsQueryable().Subset(request);

            Assert.AreEqual(subset.Total, 4);
            Assert.AreEqual(subset.Skipped, 0);
            Assert.AreEqual(subset.Taken, 0);
            Assert.AreEqual(subset.Items.Count, 4);
            Assert.AreEqual(subset.Items[0].Price, 100);
            Assert.AreEqual(subset.Items[1].Price, 100);
        }
    }
}

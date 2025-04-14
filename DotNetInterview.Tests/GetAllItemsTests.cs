using DotNetInterview.API;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using DotNetInterview.API.Domain;
using DotNetInterview.API.Query;
using DotNetInterview.Tests.TestUtilities;

namespace DotNetInterview.Tests
{
	public class GetAllItemsTests
	{
		private DataContext _dataContext;
		private SqliteConnection _connection;

		[SetUp]
		public void Setup()
		{
			_connection = new SqliteConnection("DataSource=:memory:");
			_connection.Open();

			var options = new DbContextOptionsBuilder<DataContext>()
				.UseSqlite(_connection)
				.Options;

			_dataContext = new DataContext(options);
			_dataContext.Database.EnsureCreated();

            // Clear seeded data for isolation of tests
            _dataContext.Items.RemoveRange(_dataContext.Items);
            _dataContext.SaveChanges();
        }

		[TearDown]
		public void TearDown()
		{
			_dataContext?.Dispose();
			_connection?.Close();
			_connection?.Dispose();
		}

		// to check if items get returned with correct discounts calculated on days excluding Monday
		[Test]
		public async Task GetAllItems_AppliesHighestDiscountOnNonMondays()
		{
			// mock items in db
			var item1 = new Item
			{
				Reference = "REF001",
				Name = "Item 1",
				Price = 100,
				Variations = new List<Variation>
				{
					new Variation { Size = "Small", Quantity = 4 },
					new Variation { Size = "Medium", Quantity = 2 }
				}
			};

			var item2 = new Item
			{
				Reference = "REF002",
				Name = "Item 2",
				Price = 200,
				Variations = new List<Variation>
				{
					new Variation { Size = "Large", Quantity = 12 }
				}
			};

            var item3 = new Item
            {
                Reference = "REF003",
                Name = "Item 3",
                Price = 80,
                Variations = new List<Variation>
                {
                    new Variation { Size = "Large", Quantity = 0 }
                }
            };

            var item4 = new Item
            {
                Reference = "REF004",
                Name = "Item 4",
                Price = 100,
                Variations = new List<Variation>
                {
                    new Variation { Size = "Small", Quantity = 5 }
                }
            };

            var item5 = new Item
            {
                Reference = "REF005",
                Name = "Item 5",
                Price = 200,
                Variations = new List<Variation>
                {
                    new Variation { Size = "Medium", Quantity = 10 }
                }
            };

            _dataContext.Items.AddRange(item1, item2, item3, item4, item5);
			_dataContext.SaveChanges();

            // mock time as any day except Monday
            var mockTimeProvider = new MockTimeProvider
            {
                UtcNow = new DateTime(2025, 4, 15, 13, 0, 0) // Tuesday, 1 PM UTC
            };

            // invoke the query handler
            var handler = new GetAllItemsQueryHandler(_dataContext, mockTimeProvider);
			var result = await handler.Handle(new GetAllItemsQuery(), CancellationToken.None);

            // Assert
            // check count of items to verify if list of all items get returned when items exist in db
            var items = result.ToList();
			Assert.AreEqual(5, items.Count);

            // check if discount and price after discount is computed correctly
            // stock quantity of REF001 > 5, 10% discount available. It is not Monday. Highest discount is 10%
            var retrievedItem1 = items.First(i => i.Reference == "REF001");
            Assert.AreEqual(6, retrievedItem1.StockQuantity);
            Assert.AreEqual(0.1m, retrievedItem1.HighestDiscount);
			Assert.AreEqual(90, retrievedItem1.PriceAfterDiscount);

            // stock quantity of REF002 > 10, discount available are 10% and 20%. It is not Monday. Highest discount is 20%
            var retrievedItem2 = items.First(i => i.Reference == "REF002");
            Assert.AreEqual(12, retrievedItem2.StockQuantity);
            Assert.AreEqual(0.2m, retrievedItem2.HighestDiscount);
			Assert.AreEqual(160, retrievedItem2.PriceAfterDiscount);

            // no stock quantity for item REF003. Highest discount is 0
            var retrievedItem3 = items.First(i => i.Reference == "REF003");
            Assert.AreEqual(0, retrievedItem3.StockQuantity);
            Assert.AreEqual(0.0m, retrievedItem3.HighestDiscount);
            Assert.AreEqual(80, retrievedItem3.PriceAfterDiscount);

            // stock quantity of REF004 = 5 and it is not Monday, no discount available
            var retrievedItem4 = items.First(i => i.Reference == "REF004");
            Assert.AreEqual(5, retrievedItem4.StockQuantity);
            Assert.AreEqual(0.0m, retrievedItem4.HighestDiscount);
            Assert.AreEqual(100, retrievedItem4.PriceAfterDiscount);

            // stock quantity of REF002 = 10, discount available is 10%. It is not Monday. Highest discount is 10%
            var retrievedItem5 = items.First(i => i.Reference == "REF005");
            Assert.AreEqual(10, retrievedItem5.StockQuantity);
            Assert.AreEqual(0.1m, retrievedItem5.HighestDiscount);
            Assert.AreEqual(180, retrievedItem5.PriceAfterDiscount);
        }

        // to check if items get returned with correct discounts calculated on Monday
        [Test]
		public async Task GetAllItems_AppliesHighestDiscountOnMonday()
		{
            // mock items in db
            var item1 = new Item
			{
				Reference = "REF001",
				Name = "Item 1",
				Price = 300,
				Variations = new List<Variation>
				{
					new Variation { Size = "Medium", Quantity = 15 }
				}
			};

            var item2 = new Item
            {
                Reference = "REF002",
                Name = "Item 2",
                Price = 100,
                Variations = new List<Variation>
                {
                    new Variation { Size = "Small", Quantity = 3 }
                }
            };

            var item3 = new Item
            {
                Reference = "REF003",
                Name = "Item 3",
                Price = 110,
                Variations = new List<Variation>()
            };

            var item4 = new Item
            {
                Reference = "REF004",
                Name = "Item 4",
                Price = 100,
                Variations = new List<Variation>
                {
                    new Variation { Size = "Small", Quantity = 5 }
                }
            };

            var item5 = new Item
            {
                Reference = "REF005",
                Name = "Item 5",
                Price = 200,
                Variations = new List<Variation>
                {
                    new Variation { Size = "Medium", Quantity = 10 }
                }
            };

            _dataContext.Items.AddRange(item1, item2, item3, item4, item5);
            _dataContext.SaveChanges();

            // mock time as Monday between 12pm and 5pm
            var mockTimeProvider = new MockTimeProvider
            {
                UtcNow = new DateTime(2025, 4, 14, 13, 0, 0) // April 14 2025, Monday, 1 PM
            };

            // invoke the query handler
            var handler = new GetAllItemsQueryHandler(_dataContext, mockTimeProvider);
			var result = await handler.Handle(new GetAllItemsQuery(), CancellationToken.None);

            // Assert
            // check count of items to verify if list of all items get returned when items exist in db
            var items = result.ToList();
            Assert.AreEqual(5, items.Count);

            // check if discount and price after discount is computed correctly
            // stock quantity of REF001 > 10, discount available are 10% and 20%. It's a Monday, 50% discount available. Highest discount is 50%
            var retrievedItem1 = result.First(i => i.Reference == "REF001");
            Assert.AreEqual(15, retrievedItem1.StockQuantity);
            Assert.AreEqual(0.5m, retrievedItem1.HighestDiscount);
			Assert.AreEqual(150, retrievedItem1.PriceAfterDiscount);

            // On Mondays, all items with stock quantity > 0 have 50% discount available.
            var retrievedItem2 = result.First(i => i.Reference == "REF002");
            Assert.AreEqual(3, retrievedItem2.StockQuantity);
            Assert.AreEqual(0.5m, retrievedItem2.HighestDiscount);
            Assert.AreEqual(50, retrievedItem2.PriceAfterDiscount);

            // no stock quantity for item REF003. No discount available
            var retrievedItem3 = result.First(i => i.Reference == "REF003");
            Assert.AreEqual(0, retrievedItem3.StockQuantity);
            Assert.AreEqual(0.0m, retrievedItem3.HighestDiscount);
            Assert.AreEqual(110, retrievedItem3.PriceAfterDiscount);

            // stock quantity of REF004 = 5. No discount available. It's a Monday, 50% discount available. Highest discount is 50%
            var retrievedItem4 = items.First(i => i.Reference == "REF004");
            Assert.AreEqual(5, retrievedItem4.StockQuantity);
            Assert.AreEqual(0.5m, retrievedItem4.HighestDiscount);
            Assert.AreEqual(50, retrievedItem4.PriceAfterDiscount);

            // stock quantity of REF002 = 10, discount available is 10%. It's a Monday, 50% discount available. Highest discount is 50%
            var retrievedItem5 = items.First(i => i.Reference == "REF005");
            Assert.AreEqual(10, retrievedItem5.StockQuantity);
            Assert.AreEqual(0.5m, retrievedItem5.HighestDiscount);
            Assert.AreEqual(100, retrievedItem5.PriceAfterDiscount);
        }

        // to check if stock quantity and status of item are computed correctly
        [Test]
        public async Task GetAllItems_CalculatesStockQuantityAndStatusCorrectly()
        {
            // mock items in db
            var item1 = new Item
            {
                Reference = "REF001",
                Name = "Item 1",
                Price = 150,
                Variations = new List<Variation>
                {
                    new Variation { Size = "Large", Quantity = 3 },
                    new Variation { Size = "Medium", Quantity = 2 }
                }
            };

            var item2 = new Item
            {
                Reference = "REF002",
                Name = "Item 2",
                Price = 110,
                Variations = new List<Variation>()
            };

            var item3 = new Item
            {
                Reference = "REF003",
                Name = "Item 3",
                Price = 80,
                Variations = new List<Variation>
                {
                    new Variation { Size = "Large", Quantity = 0 }
                }
            };

            _dataContext.Items.AddRange(item1, item2, item3);
            _dataContext.SaveChanges();

            // mock time
            var mockTimeProvider = new MockTimeProvider
            {
                UtcNow = new DateTime(2025, 4, 16, 13, 0, 0) // Wednesday, 1 PM UTC
            };

            // invoke the query handler
            var handler = new GetAllItemsQueryHandler(_dataContext, mockTimeProvider);
            var result = await handler.Handle(new GetAllItemsQuery(), CancellationToken.None);

            // Assert
            // if stock quantity > 0, item is 'In Stock;. Otherwise 'Sold Out'
            var retrievedItem1 = result.First(i => i.Reference == "REF001");
            Assert.AreEqual(5, retrievedItem1.StockQuantity);
            Assert.AreEqual("In Stock", retrievedItem1.StockStatus);

            var retrievedItem2 = result.First(i => i.Reference == "REF002");
            Assert.AreEqual(0, retrievedItem2.StockQuantity);
            Assert.AreEqual("Sold Out", retrievedItem2.StockStatus);

            var retrievedItem3 = result.First(i => i.Reference == "REF003");
            Assert.AreEqual(0, retrievedItem3.StockQuantity);
            Assert.AreEqual("Sold Out", retrievedItem3.StockStatus);
        }

        // checks if empty list get returned when no items exist in db
        [Test]
        public async Task GetAllItems_ReturnsEmptyList_WhenNoItemsExist()
        {
            // mock time
            var mockTimeProvider = new MockTimeProvider
            {
                UtcNow = new DateTime(2025, 4, 16, 13, 0, 0) // Wednesday, 1 PM UTC
            };

            var handler = new GetAllItemsQueryHandler(_dataContext, mockTimeProvider);
            var result = await handler.Handle(new GetAllItemsQuery(), CancellationToken.None);

            // Assert
            // check for count of items
            var items = result.ToList();
            Assert.AreEqual(0, items.Count);

            // null should not be returned. The list of items need to be empty
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        // check if InvalidOperationException/ObjectDisposedException is thrown on db connection failure
        [Test]
        public async Task GetAllItems_ThrowsException_DatabaseConnectionFailure()
        {
            // mock time
            var mockTimeProvider = new MockTimeProvider
            {
                UtcNow = new DateTime(2025, 4, 16, 13, 0, 0) // Wednesday, 1 PM UTC
            };

            // mock database connection failure by disposing the context
            _dataContext.Dispose();

            // invoke the query handler
            var handler = new GetAllItemsQueryHandler(_dataContext, mockTimeProvider);

            // check if handler responds with InvalidOperationException/ObjectDisposedException on attempting to use a disposed DbContext
            Assert.That(async () => await handler.Handle(new GetAllItemsQuery(), CancellationToken.None),
                Throws.InstanceOf<InvalidOperationException>());
        }
    }
}

using DotNetInterview.API;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using DotNetInterview.API.Domain;
using DotNetInterview.API.Query;

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

		// to check if items get returned with correct discounts calculated
		[Test]
		public async Task GetAllItems_ReturnsItemsWithCorrectDiscounts()
		{
			// create mock items
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

			_dataContext.Items.AddRange(item1, item2);
			_dataContext.SaveChanges();

			var handler = new GetAllItemsHandler(_dataContext);

			// invoke the query handler
			var result = await handler.Handle(new GetAllItemsQuery(), CancellationToken.None);

			// Assert
			// check for count of items
			var items = result.ToList();
			Assert.AreEqual(2, items.Count);

			// check if discount and price after discount is computed correctly
			// stock quantity of REF001 > 5 and it is not Monday. Highest discount is 10%
			var firstItem = items.First(i => i.Reference == "REF001");
			Assert.AreEqual(0.1m, firstItem.HighestDiscount);
			Assert.AreEqual(90, firstItem.PriceAfterDiscount);

			// stock quantity of REF001 > 10 and it is not Monday. Highest discount is 20%
			var secondItem = items.First(i => i.Reference == "REF002");
			Assert.AreEqual(0.2m, secondItem.HighestDiscount);
			Assert.AreEqual(160, secondItem.PriceAfterDiscount);
		}

        //[Test]
        //public async Task GetAllItems_AppliesHighestDiscountOnMonday()
        //{
        //    // Arrange
        //    var item = new Item
        //    {
        //        Reference = "REF003",
        //        Name = "Item 4",
        //        Price = 300,
        //        Variations = new List<Variation>
        //        {
        //            new Variation { Size = "Medium", Quantity = 15 }
        //        }
        //    };

        //    _context.Items.Add(item);
        //    _context.SaveChanges();

        //    var handler = new GetAllItemsHandler(_context);

        //    // mock time as Monday between 12pm and 5pm
        //    var mockTimeProvider = new MockTimeProvider
        //    {
        //        UtcNow = new DateTime(2025, 4, 14, 13, 0, 0) // Monday, 1 PM
        //    };

        //    // Act
        //    var result = await handler.Handle(new GetAllItemsQuery(), CancellationToken.None);

        //    // Assert
        //    var retrievedItem = result.First(i => i.Reference == "REF003");
        //    Assert.AreEqual(0.5m, retrievedItem.HighestDiscount);
        //    Assert.AreEqual(150, retrievedItem.PriceAfterDiscount);
        //}

    }
}

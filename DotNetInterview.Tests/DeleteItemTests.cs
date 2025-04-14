using DotNetInterview.API;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using DotNetInterview.API.Domain;
using DotNetInterview.API.Query;
using DotNetInterview.Tests.TestUtilities;

namespace DotNetInterview.Tests
{
    public class DeleteItemTests
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

        // checks if true gets returned if item exists in db
        [Test]
        public async Task DeleteItem_ReturnsTrue_ForExistingItem()
        {
            // requested itemId
            var itemId = Guid.NewGuid();

            // mock item to be found in db
            var item1 = new Item
            {
                Id = itemId,
                Reference = "REF001",
                Name = "Item 1",
                Price = 150,
                Variations = new List<Variation>
                {
                    new Variation { Size = "Large", Quantity = 3 },
                    new Variation { Size = "Medium", Quantity = 2 }
                }
            };

            _dataContext.Items.Add(item1);
            _dataContext.SaveChanges();

            // invoke the query handler
            var handler = new DeleteItemQueryHandler(_dataContext);
            var result = await handler.Handle(new DeleteItemQuery(itemId), CancellationToken.None);

            // Assert
            //checks if item exists in db
            Assert.IsNotNull(context.Items.Find(itemId));

            // check if item in response is not null 
            Assert.IsNotNull(result);

            // check if response is true
            Assert.IsTrue(result);
        }

        // checks if false gets returned if item does not exist in db
        [Test]
        public async Task DeleteItem_ReturnsFalse_WhenItemNotFound()
        {
            // requested itemId
            var itemId = Guid.NewGuid();

            // invoke the query handler
            var handler = new DeleteItemQueryHandler(_dataContext);
            var result = await handler.Handle(new GDeleteItemQuery(itemId), CancellationToken.None);

            // Assert
            //checks if item exists in db
            Assert.IsNull(context.Items.Find(itemId));

            // check if item in response is not null 
            Assert.IsNotNull(result);

            // check if response is false
            Assert.IsFalse(result);
        }

        // check if InvalidOperationException/ObjectDisposedException is thrown on db connection failure
        [Test]
        public async Task DeleteItem_ThrowsException_DatabaseConnectionFailure()
        {
            // requested itemId
            var itemId = Guid.NewGuid();

            // mock database connection failure by disposing the context
            _dataContext.Dispose();

            // invoke the query handler
            var handler = new DeleteItemQueryHandler(_dataContext);

            // Assert
            // check if handler responds with InvalidOperationException/ObjectDisposedException on attempting to use a disposed DbContext
            Assert.That(async () => await handler.Handle(new DeleteItemQuery(itemId), CancellationToken.None),
                Throws.InstanceOf<InvalidOperationException>());
        }
    }
}

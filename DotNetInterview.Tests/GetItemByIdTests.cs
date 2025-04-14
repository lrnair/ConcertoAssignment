using DotNetInterview.API;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using DotNetInterview.API.Domain;
using DotNetInterview.API.Query;
using DotNetInterview.Tests.TestUtilities;

namespace DotNetInterview.Tests
{
    public class GetItemByIdTests
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

        // checks if item gets returned when item is found in db
        [Test]
        public async Task GetItemById_ReturnsItem_WhenItemFound()
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
            var handler = new GetItemByIdQueryHandler(_dataContext);
            var result = await handler.Handle(new GetItemByIdQuery(itemId), CancellationToken.None);

            // Assert
            // check if item in response is not null 
            Assert.IsNotNull(result);

            // verify item in response
            Assert.AreEqual(itemId, result.Id);
            Assert.AreEqual("Item 1", result.Name);
            // check if handler correctly retrieves multiple variations of the item
            Assert.AreEqual(2, result.Variations.Count);
        }

        // checks if null gets returned when item is found in db
        [Test]
        public async Task GetItemById_ReturnsNull_WhenItemNotFound()
        {
            // requested itemId
            var itemId = Guid.NewGuid();

            // invoke the query handler
            var handler = new GetItemByIdQueryHandler(_dataContext);
            var result = await handler.Handle(new GetItemByIdQuery(itemId), CancellationToken.None);

            // Assert
            // check if item in response is null 
            Assert.IsNull(result);
        }

        // check if InvalidOperationException/ObjectDisposedException is thrown on db connection failure
        [Test]
        public async Task GetItemById_ThrowsException_DatabaseConnectionFailure()
        {
            // requested itemId
            var itemId = Guid.NewGuid();

            // mock database connection failure by disposing the context
            _dataContext.Dispose();

            // invoke the query handler
            var handler = new GetItemByIdQueryHandler(_dataContext);

            // check if handler responds with InvalidOperationException/ObjectDisposedException on attempting to use a disposed DbContext
            Assert.That(async () => await handler.Handle(new GetItemByIdQuery(itemId), CancellationToken.None),
                Throws.InstanceOf<InvalidOperationException>());
        }
    }
}

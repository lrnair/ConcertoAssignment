using DotNetInterview.API;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using DotNetInterview.API.Domain;
using DotNetInterview.API.DTO;
using DotNetInterview.API.Command;
using DotNetInterview.Tests.TestUtilities;

namespace DotNetInterview.Tests
{
    public class CreateItemTests
    {
        private DataContext _dataContext;
        private SqliteConnection _connection;
        private DataContext _mockDataContext;

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

        // checks if valid item gets created successfully
        [Test]
        public async Task CreateItem_CreatesItemSuccessfully_ValidCommand()
        {
            // valid item to be created
            var item = new CreateItemCommand
            {
                Reference = "REF001",
                Name = "Test Item",
                Price = 99.99m,
                Variations = new List<VariationDto>
                {
                    new VariationDto { Size = "Large", Quantity = 5 }
                }
            };

            // invoke the command handler
            var handler = new CreateItemCommandHandler(_dataContext);
            var result = await handler.Handle(item, CancellationToken.None);

            // Assert
            // check if item in response is not null 
            Assert.IsNotNull(result);

            // check item in response to verify successful item creation
            Assert.AreEqual(item.Reference, result.Reference);
            Assert.AreEqual(item.Name, result.Name);
            Assert.AreEqual(item.Price, result.Price);
            Assert.AreEqual(1, result.Variations.Count);
            Assert.AreEqual("Large", result.Variations.First().Size);
            Assert.AreEqual(5, result.Variations.First().Quantity);
        }

        // checks if valid item without variations gets created successfully
        [Test]
        public async Task CreateItem_CreatesItemWithEmptyVariations()
        {
            // valid item to be created - without variations
            var item = new CreateItemCommand
            {
                Reference = "REF001",
                Name = "Item Without Variations",
                Price = 49.99m
            };

            // invoke the command handler
            var handler = new CreateItemCommandHandler(_dataContext);
            var result = await handler.Handle(item, CancellationToken.None);

            // Assert
            // check if item in response is not null 
            Assert.IsNotNull(result);

            // check item in response to verify successful item creation
            Assert.AreEqual(item.Reference, result.Reference);
            Assert.AreEqual(item.Name, result.Name);
            Assert.AreEqual(item.Price, result.Price);

            //check if variation is empty and not null
            Assert.IsNotNull(result.Variations);
            Assert.IsEmpty(result.Variations);
        }

        // checks if item creation fails for invalid item - negative quantity
        [Test]
        public async Task CreateItem_ThrowsException_InvalidVariationData()
        {
            // invalid item to be created - negative quantity
            var item = new CreateItemCommand
            {
                Reference = "REF001",
                Name = "Item with Invalid Variation",
                Price = 49.99m,
                Variations = new List<VariationDto>
                {
                    new VariationDto { Size = "Medium", Quantity = -5 }
                }
            };

            // invoke the command handler
            var handler = new CreateItemCommandHandler(_dataContext);

            // Assert
            // check if handler responds with ArgumentException
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(item, CancellationToken.None));
            Assert.That(ex.Message, Is.EqualTo("Quantity must be zero or a positive number."));
        }
    }
}

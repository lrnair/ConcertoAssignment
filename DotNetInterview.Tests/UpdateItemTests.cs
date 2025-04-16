using DotNetInterview.API;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using DotNetInterview.API.Domain;
using DotNetInterview.API.DTO;
using DotNetInterview.API.Command;
using DotNetInterview.Tests.TestUtilities;

namespace DotNetInterview.Tests
{
    public class UpdateItemTests
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
            _dataContext.Database.EnsureDeleted();
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

        // checks if valid item gets updated successfully
        [Test]
        public async Task UpdateItem_UpdatesItemSuccessfully_ValidCommand()
        {
            // itemId to be updated
            var itemId = Guid.NewGuid();

            // valid item to be updated
            var variationId = Guid.NewGuid();
            var item = new Item
            {
                Id = itemId,
                Reference = "REF001",
                Name = "Test Item",
                Price = 99.99m
            };

            _dataContext.Items.Add(item);
            await _dataContext.SaveChangesAsync();

            // mock updations to existing item           
            var command = new UpdateItemCommand(itemId, "REF002", "Updated Item", 75.00m, new List<VariationDto>());

            // invoke the command handler          
            var handler = new UpdateItemCommandHandler(_dataContext);
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            // verify updations
            Assert.IsNotNull(result);
            Assert.AreEqual("REF002", result.Reference);
            Assert.AreEqual("Updated Item", result.Name);
            Assert.AreEqual(75.00m, result.Price);
            Assert.AreEqual(0, result.Variations.Count);
        }

        // checks if null gets returned on updation of an item that is not in db
        [Test]
        public async Task UpdateItem_ReturnsNull_ItemNotFound()
        {
            // mock itemId that is not in db
            var nonExistentItemId = Guid.NewGuid();

            // invoke the command handler
            var command = new UpdateItemCommand(nonExistentItemId, "REF003", "Non-Existent Item", 80.00m, new List<VariationDto>());
            var handler = new UpdateItemCommandHandler(_dataContext);
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            //check if null is returned
            Assert.IsNull(result);
        }

        // checks if existing variation gets removed
        [Test]
        public async Task UpdateItem_RemovesExistingVariationsSuccessfully()
        {
            // itemId to be updated
            var itemId = Guid.NewGuid();

            // valid item to be updated
            var variationId = Guid.NewGuid();
            var item = new Item
            {
                Id = itemId,
                Reference = "REF001",
                Name = "Test Item",
                Price = 99.99m,
                Variations = new List<Variation>
                {
                    new Variation { Id = variationId, ItemId = itemId, Size = "Large", Quantity = 5 }
                }
            };

            _dataContext.Items.Add(item);
            _dataContext.SaveChanges();

            // mock updations to existing item
            var command = new UpdateItemCommand(itemId, "REF001", "Test Item", 99.99m, new List<VariationDto>());

            // invoke the command handler
            var handler = new UpdateItemCommandHandler(_dataContext);
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            // verify if existing variation is removed
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Variations.Count);
        }

        // checks if new variation gets added
        [Test]
        public async Task UpdateItem_AddsVariationsSuccessfully_AddNewVariations()
        {
            // itemId to be updated
            var itemId = Guid.NewGuid();

            // valid item to be updated
            var variationId = Guid.NewGuid();
            var item = new Item
            {
                Id = itemId,
                Reference = "REF001",
                Name = "Test Item",
                Price = 99.99m,
                Variations = new List<Variation>
                {
                    new Variation { Id = variationId, ItemId = itemId, Size = "Large", Quantity = 5 }
                }
            };

            _dataContext.Items.Add(item);
            _dataContext.SaveChanges();

            // mock updations to existing item
            item.Variations.Clear();
            var updatedVariations = new List<VariationDto>
            {
                new VariationDto { Id = variationId, ItemId = itemId, Size = "Large", Quantity = 20 },
                new VariationDto { Size = "Extra Large", Quantity = 15 }
            };
            var command = new UpdateItemCommand(itemId, "REF002", "Updated Item", 75.00m, updatedVariations);

            // invoke the command handler
            var handler = new UpdateItemCommandHandler(_dataContext);
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            // verify if new variation is added
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Variations.Count);          
        }

    }
}

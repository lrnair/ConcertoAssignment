using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetInterview.API.Controller;
using DotNetInterview.API.DTO;
using DotNetInterview.API.Domain;
using DotNetInterview.API.Query;
using DotNetInterview.API.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace DotNetInterview.Tests
{
	public class ItemsControllerTests
	{
		private Mock<IMediator> _mediatorMock;
		private ItemsController _controller;

		[SetUp]
		public void Setup()
		{
			_mediatorMock = new Mock<IMediator>();
			_controller = new ItemsController(_mediatorMock.Object);
		}

		// GetAllItems API - Returns Ok with list of items if Items are available in db
		[Test]
		public async Task GetAllItems_ReturnsOkResult_WithListOfItems()
		{
            // mock list of items to be returned from handler
            var mockItems = new List<GetAllItemsDto>
			{
				new GetAllItemsDto
				{
					Id = Guid.NewGuid(),
					Reference = "REF001",
					Name = "Item 1",
					Price = 100,
					HighestDiscount = 0.1m,
					PriceAfterDiscount = 90,
					StockQuantity = 6,
					StockStatus = "In Stock"
				},
				new GetAllItemsDto
				{
					Id = Guid.NewGuid(),
					Reference = "REF002",
					Name = "Item 2",
					Price = 200,
					HighestDiscount = 0.2m,
					PriceAfterDiscount = 160,
					StockQuantity = 12,
					StockStatus = "In Stock"
				}
			};

            // mock IMediator to return list of items from handler
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllItemsQuery>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(mockItems);

			// invoke the GetAllItems API
			var result = await _controller.GetAllItems();

			// Assert
			// check if response is OK
			Assert.IsInstanceOf<OkObjectResult>(result.Result);
			var okResult = result.Result as OkObjectResult;
			Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            // check count of items in response
            var items = okResult.Value as IEnumerable<GetAllItemsDto>;
			Assert.IsNotNull(items);
			Assert.AreEqual(2, items.Count());

			// verify item in response
			var firstItem = items.First();
			Assert.AreEqual("REF001", firstItem.Reference);
			Assert.AreEqual(0.1m, firstItem.HighestDiscount);
			Assert.AreEqual(90, firstItem.PriceAfterDiscount);
			Assert.AreEqual("In Stock", firstItem.StockStatus);
		}

		// GetAllItems API - Returns Ok with empty list if no Items are available in db
		[Test]
		public async Task GetAllItems_ReturnsEmptyList_WhenNoItemsAvailable()
		{
            // mock IMediator to return empty list of items from handler
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllItemsQuery>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new List<GetAllItemsDto>());

			// invoke the GetAllItems API
			var result = await _controller.GetAllItems();

			// Assert
			// check if response is OK
			Assert.IsInstanceOf<OkObjectResult>(result.Result);
			var okResult = result.Result as OkObjectResult;
			Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            // check if response is empty list and not null
            var items = okResult.Value as IEnumerable<GetAllItemsDto>;
			Assert.IsNotNull(items);
			Assert.IsEmpty(items);
		}

		// GetAllItems API - Handles mediator exceptions with 500 status code
		[Test]
		public async Task GetAllItems_HandlesMediatorException()
		{
			// mocks mediator to throw an exception
			_mediatorMock
				.Setup(m => m.Send(It.IsAny<GetAllItemsQuery>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception("Unexpected error"));

			// invoke the GetAllItems API
			var result = await _controller.GetAllItems();

			// Assert
			// check if API handles mediator exception with 500 error and message
			Assert.IsInstanceOf<ObjectResult>(result.Result);
			var objectResult = result.Result as ObjectResult;
			Assert.IsNotNull(objectResult);
			Assert.AreEqual(500, objectResult.StatusCode);
			Assert.AreEqual("An error occurred while processing your request.", objectResult.Value);
		}

        // GetItemById API - Returns Ok with requested item from db
        [Test]
        public async Task GetItemById_ReturnsOkResult_WithRequestedItem()
        {
			// specify requested itemId and its variationIds
			var itemId = Guid.NewGuid();
			var variation1Id = Guid.NewGuid();
			var variation2Id = Guid.NewGuid();

            // mock item to be returned from handler
            var expectedItem = new Item
            {
                Id = itemId,
                Reference = "REF001",
                Name = "Item 1",
                Price = 100,
                Variations = new List<Variation>
				{
				    new Variation
				    {
				        Id = variation1Id,
				        ItemId = itemId,
				        Size = "Small",
				        Quantity = 4
				    },
				    new Variation
				    {
				        Id = variation2Id,
				        ItemId = itemId,
				        Size = "Small",
				        Quantity = 4
				    }
				}
            };

            // mock IMediator to return requested item from handler
            _mediatorMock
                .Setup(m => m.Send(It.Is<GetItemByIdQuery>(q => q.Id == itemId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedItem);

            // invoke the GetItemById API
            var result = await _controller.GetItemById(itemId.ToString());

            // Assert
            // check if response is OK
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            // verify item in response
            Assert.AreEqual(expectedItem, okResult.Value);
        }

        // GetItemById API - Returns Not Found if requested item not in db
        [Test]
        public async Task GetItemById_ReturnsNotFound_WhenItemDoesNotExist()
        {
            // specify requested itemId
            var itemId = Guid.NewGuid();

            // mock IMediator to return requested item from handler
            _mediatorMock
                .Setup(m => m.Send(It.Is<GetItemByIdQuery>(q => q.Id == itemId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Item)null);

            // invoke the GetItemById API
            var result = await _controller.GetItemById(itemId.ToString());

            // Assert
            // check if response is Not Found
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
            var notFoundResult = result.Result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        // GetItemById API - Handles mediator exceptions with 500 status code
        [Test]
        public async Task GetItemById_HandlesMediatorException()
        {
            // specify item to be returned from handler
            var itemId = Guid.NewGuid();

            // mocks mediator to throw an exception
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetItemByIdQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // invoke the GetItemById API
            var result = await _controller.GetItemById(itemId.ToString());

            // Assert
            // check if API handles mediator exception with 500 error and message
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("An error occurred while processing your request.", objectResult.Value);
        }

        // GetItemById API - Returns Bad Request when invalid itemId provided
        public async Task GetItemById_ReturnsBadRequest_InvalidItemId()
        {
            // mock an invalid itemId. itemId is expected to be a valid GUID
            var itemId = "invalid-guid";

            // invoke the GetItemById API
            var result = await _controller.GetItemById(itemId);

            // Assert
            // check if response is Bad Request
            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid item Id. Please provide a valid GUID as the Id", badRequestResult.Value);
        }

        // CreateItem API - Returns 201 Created with the correct item details from db
        [Test]
        public async Task CreateItem_ReturnsCreatedItem_ValidRequest()
        {
            // specify item to be created
            var item = new CreateItemCommand
            {
                Reference = "REF001",
                Name = "Test Item",
                Price = 99.99m,
                Variations = new List<VariationDto>
                {
                    new VariationDto { Size = "Medium", Quantity = 10 }
                }
            };

            // mock the correct item details to be returned from handler
            var expectedItem = new ItemDto
            {
                Id = Guid.NewGuid(),
                Reference = item.Reference,
                Name = item.Name,
                Price = item.Price,
                Variations = item.Variations
            };

            // mock IMediator to invoke CreateItem API to create the item requested and return correct item details from db
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateItemCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedItem);
            var result = await _controller.CreateItem(item);

            // Assert
            // check if response is 201 Created
            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
            var createdAtResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdAtResult);
            Assert.AreEqual(201, createdAtResult.StatusCode);

            // verify action invoked and the returned item details
            Assert.AreEqual(expectedItem, createdAtResult.Value);
            Assert.AreEqual("GetItemById", createdAtResult.ActionName);
            Assert.AreEqual(expectedItem.Id, createdAtResult.RouteValues["id"]);
        }

        // CreateItem API - Returns 201 Created with the correct item details - no variations from db
        [Test]
        public async Task CreateItem_ReturnsCreatedItem_ValidRequestWithoutVariations()
        {
            // specify item to be created - without any variations
            var item = new CreateItemCommand
            {
                Reference = "REF001",
                Name = "Test Item",
                Price = 99.99m
            };

            // mock the correct item details to be returned from handler
            var expectedItem = new ItemDto
            {
                Id = Guid.NewGuid(),
                Reference = item.Reference,
                Name = item.Name,
                Price = item.Price,
                Variations = new List<VariationDto>()
            };

            // mock IMediator to invoke CreateItem API to create the item requested and return correct item details from db
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateItemCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedItem);
            var result = await _controller.CreateItem(item);

            // Assert
            // check if response is 201 Created
            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
            var createdAtResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdAtResult);
            Assert.AreEqual(201, createdAtResult.StatusCode);

            // verify action invoked and the returned item details
            Assert.AreEqual(expectedItem, createdAtResult.Value);
            Assert.AreEqual("GetItemById", createdAtResult.ActionName);
            Assert.AreEqual(expectedItem.Id, createdAtResult.RouteValues["id"]);
        }

        // CreateItem API - Returns Bad Request for invalid item - with required fields missing
        [Test]
        public async Task CreateItem_ReturnsBadRequest_InvalidModelState()
        {
            // specify item to be created - with required fields missing
            var item = new CreateItemCommand
            {
                Name = "Test Item",
                Price = 99.99m
            };

            // mock controller to throw missing field error
            _controller.ModelState.AddModelError("Reference", "Reference is required");

            // invoke the CreateItem API
            var result = await _controller.CreateItem(item);

            // Assert
            // check if response is Bad Request
            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid item. Please provide all required fields.", badRequestResult.Value);
        }

        // CreateItem API - Handles mediator exceptions with 500 status code
        [Test]
        public async Task CreateItem_HandlesMediatorException()
        {
            // specify item to be created
            var item = new CreateItemCommand
            {
                Reference = "REF001",
                Name = "Test Item",
                Price = 99.99m,
                Variations = new List<VariationDto>
                {
                    new VariationDto { Size = "Medium", Quantity = 10 }
                }
            };

            // mocks mediator to throw an exception
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateItemCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // invoke the CreateItem API
            var result = await _controller.CreateItem(item);

            // Assert
            // check if API handles mediator exception with 500 error and message
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("An error occurred while processing your request.", objectResult.Value);
        }

        // DeleteItem API - Returns Ok if item to be deleted exists in db
        [Test]
        public async Task DeleteItem_ReturnsOk_ForExistingItem()
        {
            // specify item to be deleted
            var itemId = Guid.NewGuid();

            // mock IMediator to return true from handler after deleting existing item
            _mediatorMock
                .Setup(m => m.Send(It.Is<DeleteItemCommand>(cmd => cmd.Id == itemId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // invoke the DeleteItem API
            var result = await _controller.DeleteItem(itemId.ToString());

            // Assert
            // check if response is OK with expected message
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsTrue(okResult.Value.ToString().Contains($"Item with ID {itemId} deleted successfully."));
        }

        // DeleteItem API - Returns Not Found if item to be deleted do not exist in db
        [Test]
        public async Task DeleteItem_ReturnsNotFound_WhenItemDoesNotExist()
        {
            // specify item to be deleted
            var itemId = Guid.NewGuid();

            // mock IMediator to return false from handler if item to be deleted do not exist in db
            _mediatorMock
                .Setup(m => m.Send(It.Is<DeleteItemCommand>(cmd => cmd.Id == itemId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // invoke the DeleteItem API
            var result = await _controller.DeleteItem(itemId.ToString());

            // Assert
            // check if response is Not Found
            Assert.IsInstanceOf<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        // DeleteItem API - Handles mediator exceptions with 500 status code
        [Test]
        public async Task DeleteItem_HandlesMediatorException()
        {
            // specify item to be returned from handler
            var itemId = Guid.NewGuid();

            // mocks mediator to throw an exception
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<DeleteItemCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // invoke the DeleteItem API
            var result = await _controller.DeleteItem(itemId.ToString());

            // Assert
            // check if API handles mediator exception with 500 error and message
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("An error occurred while processing your request.", objectResult.Value);
        }

        // DeleteItem API - Returns Bad Request when invalid itemId provided
        public async Task DeleteItem_ReturnsBadRequest_InvalidItemId()
        {
            // mock an invalid itemId. itemId is expected to be a valid GUID
            var itemId = "invalid-guid";

            // invoke the GetItemById API
            var result = await _controller.DeleteItem(itemId);

            // Assert
            // check if response is Bad Request
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid item Id. Please provide a valid GUID as the Id", badRequestResult.Value);
        }
    }
}

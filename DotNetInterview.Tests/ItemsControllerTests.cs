using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetInterview.API.Controller;
using DotNetInterview.API.DTO;
using DotNetInterview.API.Domain;
using DotNetInterview.API.Query;
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
				.ThrowsAsync(new Exception("Mediator error"));

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
			// requested itemId and its variationIds
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
            // requested itemId
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
        public async Task GetItemById_ReturnsInternalServerError_OnException()
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
        public async Task GetItemById_InvalidItemId_ReturnsBadRequest()
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

    }
}

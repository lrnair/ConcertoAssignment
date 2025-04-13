using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetInterview.API.Controller;
using DotNetInterview.API.DTO;
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

		// GetAllItems API - Returns Ok with all Items available
		[Test]
		public async Task GetAllItems_ReturnsOkResult_WithListOfItems()
		{
			// create mock items
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

		// GetAllItems API - Returns Ok if no Items available
		[Test]
		public async Task GetAllItems_ReturnsEmptyList_WhenNoItemsAvailable()
		{
			// empty mock item list provided
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

			// check if response is empty
			var items = okResult.Value as IEnumerable<GetAllItemsDto>;
			Assert.IsNotNull(items);
			Assert.IsEmpty(items);
		}

		// GetAllItems API - Handles Mediator Exceptions
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
	}
}

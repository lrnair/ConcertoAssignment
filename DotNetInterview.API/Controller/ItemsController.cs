using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetInterview.API.Domain;
using DotNetInterview.API.DTO;
using DotNetInterview.API.Query;
using DotNetInterview.API.Command;

namespace DotNetInterview.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ItemsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // List all items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetAllItemsDto>>> GetAllItems()
        {
            var items = await _mediator.Send(new GetAllItemsQuery());
            return Ok(items);
        }

        // Get a single item
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItemById(Guid id)
        {
            var item = await _mediator.Send(new GetItemByIdQuery(id));
            if (item == null)
                return NotFound();  // 404 Not Found status code if the requested item could not be found in db

            return Ok(item);
        }

        // Create a new item
        [HttpPost]
        public async Task<ActionResult<ItemDto>> CreateItem([FromBody] CreateItemCommand command)
        {
            var item = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetItemById), new { id = item.Id }, item);    // returns created data from db with 201 Created status code
        }

        // Update an item
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateItemCommand command)
        {
            if (id != command.Id)
                return BadRequest("Id in URL does not match Id in body.");

            var item = await _mediator.Send(command);
            if (item == null)
                return NotFound();  // 404 Not Found status code if the item under update could not be found in db 

            return Ok(item);
        }
    }
}
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
            try
            {
                var items = await _mediator.Send(new GetAllItemsQuery());
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // Get a single item
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItemById(string id)
        {
            if (!Guid.TryParse(id, out var guid))
            {
                return BadRequest("Invalid item Id. Please provide a valid GUID as the Id");
            }

            try
            {               
                var item = await _mediator.Send(new GetItemByIdQuery(guid));
                if (item == null)
                    return NotFound();  // 404 Not Found status code if the requested item could not be found in db

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // Create a new item
        [HttpPost]
        public async Task<ActionResult<ItemDto>> CreateItem([FromBody] CreateItemCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid item. Please provide all required fields.");
            }

            try
            {
                var item = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetItemById), new { id = item.Id }, item);    // returns created data from db with 201 Created status code
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // Update an item
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateItemCommand command)
        {
            if (id != command.Id)
            { 
                return BadRequest("Id in URL does not match Id in body.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid item. Please provide all required fields.");
            }

            try
            {
                var item = await _mediator.Send(command);
                if (item == null)
                    return NotFound();  // 404 Not Found status code if the item under update could not be found in db 

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // Delete an item
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(string id)
        {
            if (!Guid.TryParse(id, out var guid))
            {
                return BadRequest("Invalid item Id. Please provide a valid GUID as the Id");
            }

            try
            {                
                var result = await _mediator.Send(new DeleteItemCommand(guid));
                if (!result)
                    return NotFound();  //  404 Not Found status code if the item for deletion could not be found in db 

                return Ok(new { message = $"Item with ID {id} deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
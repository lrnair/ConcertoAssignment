using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetInterview.API.Domain;
using DotNetInterview.API.Query;

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
        public async Task<ActionResult<IEnumerable<Item>>> GetAllItems()
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
                return NotFound();

            return Ok(item);
        }
    }
}
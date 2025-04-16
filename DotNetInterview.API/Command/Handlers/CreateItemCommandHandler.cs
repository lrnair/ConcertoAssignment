using MediatR;
using DotNetInterview.API.Domain;
using DotNetInterview.API.DTO;
using Microsoft.EntityFrameworkCore;

namespace DotNetInterview.API.Command
{
    public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, ItemDto>
    {
        private readonly DataContext _context;

        public CreateItemCommandHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<ItemDto> Handle(CreateItemCommand request, CancellationToken cancellationToken)
        {
            // validate variations in request
            if (request.Variations != null)
            {
                foreach (var variation in request.Variations)
                {
                    if (variation.Quantity < 0)
                    {
                        throw new ArgumentException("Quantity must be zero or a positive number.");
                    }
                }
            }

            var itemId = Guid.NewGuid();

            var item = new Item
            {
                Id = itemId,
                Reference = request.Reference,
                Name = request.Name,
                Price = request.Price,
                Variations = request.Variations?.Select(v => new Variation
                {
                    Id = Guid.NewGuid(),                
                    ItemId = itemId,
                    Size = v.Size,
                    Quantity = v.Quantity
                }).ToList() ?? new List<Variation>()    // adding Variations to an Item is optional
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync(cancellationToken);

            return new ItemDto
            {
                Id = item.Id,
                Reference = item.Reference,
                Name = item.Name,
                Price = item.Price,
                Variations = item.Variations.Select(v => new VariationDto
                {
                    Size = v.Size,
                    Quantity = v.Quantity
                }).ToList()
            };
        }
    }
}

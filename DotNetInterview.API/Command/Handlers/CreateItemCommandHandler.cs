using MediatR;
using DotNetInterview.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace DotNetInterview.API.Command
{
    public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, Item>
    {
        private readonly DataContext _context;

        public CreateItemCommandHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Item> Handle(CreateItemCommand request, CancellationToken cancellationToken)
        {
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

            return item;
        }
    }
}
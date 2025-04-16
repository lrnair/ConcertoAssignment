using MediatR;
using DotNetInterview.API.Domain;
using DotNetInterview.API.DTO;
using Microsoft.EntityFrameworkCore;

namespace DotNetInterview.API.Command
{
    public class UpdateItemCommandHandler : IRequestHandler<UpdateItemCommand, ItemDto>
    {
        private readonly DataContext _context;

        public UpdateItemCommandHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<ItemDto> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
        {
            var item = await _context.Items
                .Include(i => i.Variations)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (item == null)
                return null;

            // Reference, Name and Price of an item are mandatory fields
            item.Reference = request.Reference;
            item.Name = request.Name;
            item.Price = request.Price;

            // Variations are optional. Can be updated or deleted.

            // Delete existing Variations
            // Create a list of Variation Ids in request
            var requestVariationIds = request.Variations
                .Where(v => v.Id.HasValue && v.Id.Value != Guid.Empty)
                .Select(v => v.Id)
                .ToList();

            // Remove Variations from db that don't exist in the request
            var variationsToRemove = item.Variations
                .Where(v => !requestVariationIds.Contains(v.Id))
                .ToList();

            foreach (var variation in variationsToRemove)
            {
                item.Variations.Remove(variation);
            }

            // Update existing Variations
            foreach (var variation in request.Variations)
            {
                if (variation.Id.HasValue && variation.Id.Value != Guid.Empty)
                {
                    var existingVariation = item.Variations.FirstOrDefault(v => v.Id == variation.Id);
                    if (existingVariation != null)
                    {
                        existingVariation.Size = variation.Size;
                        existingVariation.Quantity = variation.Quantity;
                    }
                }
            }

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

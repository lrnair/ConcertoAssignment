using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetInterview.API.DTO;

namespace DotNetInterview.API.Query
{
    public class GetItemByIdQueryHandler : IRequestHandler<GetItemByIdQuery, ItemDto>
    {
        private readonly DataContext _context;

        public GetItemByIdQueryHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<ItemDto> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
        {
            var item = await _context.Items
                     .Include(i => i.Variations)
                     .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (item == null)
                return null;

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
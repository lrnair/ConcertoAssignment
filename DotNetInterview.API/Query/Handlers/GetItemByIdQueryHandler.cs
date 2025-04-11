using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetInterview.API.Domain;

namespace DotNetInterview.API.Query
{
    public class GetItemByIdQueryHandler : IRequestHandler<GetItemByIdQuery, Item>
    {
        private readonly DataContext _context;

        public GetItemByIdQueryHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Item> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.Items
                                 .Include(i => i.Variations)
                                 .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        }
    }
}
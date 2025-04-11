using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetInterview.API.Domain;

namespace DotNetInterview.API.Query
{
    public class GetAllItemsHandler : IRequestHandler<GetAllItemsQuery, IEnumerable<Item>>
    {
        private readonly DataContext _context;

        public GetAllItemsHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Item>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Items
                                 .Include(i => i.Variations)
                                 .ToListAsync(cancellationToken);
        }
    }
}
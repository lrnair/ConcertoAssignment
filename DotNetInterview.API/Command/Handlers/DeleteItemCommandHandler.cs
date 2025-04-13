using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetInterview.API.Command
{
    public class DeleteItemCommandHandler : IRequestHandler<DeleteItemCommand, bool>
    {
        private readonly DataContext _context;

        public DeleteItemCommandHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
        {
            var item = await _context.Items
                .Include(i => i.Variations)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (item == null)
                return false;

            _context.Items.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

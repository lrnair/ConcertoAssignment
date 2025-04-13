using MediatR;

namespace DotNetInterview.API.Command
{
    public class DeleteItemCommand : IRequest<bool>
    {
        public Guid Id { get; }

        public DeleteItemCommand(Guid id)
        {
            Id = id;
        }
    }
}

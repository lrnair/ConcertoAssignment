using MediatR;
using System.Collections.Generic;
using DotNetInterview.API.DTO;

namespace DotNetInterview.API.Query
{
	public record GetItemByIdQuery : IRequest<ItemDto>
    {
        public Guid Id { get; }

        public GetItemByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
using MediatR;
using System.Collections.Generic;
using DotNetInterview.API.Domain;

namespace DotNetInterview.API.Query
{
	public record GetItemByIdQuery : IRequest<Item>
    {
        public Guid Id { get; }

        public GetItemByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
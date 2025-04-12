using MediatR;
using System.Collections.Generic;
using DotNetInterview.API.Domain;

namespace DotNetInterview.API.Query
{
	public record GetAllItemsQuery : IRequest<IEnumerable<Item>>;
}
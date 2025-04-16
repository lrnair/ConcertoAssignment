using MediatR;
using System.Collections.Generic;
using DotNetInterview.API.DTO;

namespace DotNetInterview.API.Query
{
	public record GetAllItemsQuery : IRequest<IEnumerable<GetAllItemsDto>>;
}

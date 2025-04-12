using MediatR;
using DotNetInterview.API.Domain;

namespace DotNetInterview.API.Command
{
    public class CreateItemCommand : IRequest<Item>
    {
        public string Reference { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public List<Variation>? Variations { get; set; }
    }
}
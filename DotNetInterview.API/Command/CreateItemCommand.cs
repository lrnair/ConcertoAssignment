using MediatR;
using DotNetInterview.API.DTO;

namespace DotNetInterview.API.Command
{
    public class CreateItemCommand : IRequest<ItemDto>
    {
        public string Reference { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public ICollection<VariationDto> Variations { get; set; } = new List<VariationDto>();

        public CreateItemCommand(string reference, string name, decimal price, ICollection<VariationDto> variations)
        {
            Reference = reference;
            Name = name;
            Price = price;
            Variations = variations;
        }
    }
}
using MediatR;
using DotNetInterview.API.DTO;

namespace DotNetInterview.API.Command
{
    public class UpdateItemCommand : IRequest<ItemDto>
    {
        public Guid Id { get; }
        public string Reference { get; }
        public string Name { get; }
        public decimal Price { get; }
        public ICollection<VariationDto> Variations { get; }

        public UpdateItemCommand(Guid id, string reference, string name, decimal price, ICollection<VariationDto> variations)
        {
            Id = id;
            Reference = reference;
            Name = name;
            Price = price;
            Variations = variations ?? new List<VariationDto>();
        }
    }
}
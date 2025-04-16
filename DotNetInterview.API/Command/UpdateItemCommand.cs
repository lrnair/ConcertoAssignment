using MediatR;
using DotNetInterview.API.DTO;
using System.ComponentModel.DataAnnotations;

namespace DotNetInterview.API.Command
{
    public class UpdateItemCommand : IRequest<ItemDto>
    {
        public Guid Id { get; set; }

        [Required]
        public string Reference { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        public ICollection<VariationDto> Variations { get; set; }

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

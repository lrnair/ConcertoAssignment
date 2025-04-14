using MediatR;
using DotNetInterview.API.DTO;
using System.ComponentModel.DataAnnotations;

namespace DotNetInterview.API.Command
{
    public class CreateItemCommand : IRequest<ItemDto>
    {
        [Required]
        public string Reference { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        public ICollection<VariationDto> Variations { get; set; } = new List<VariationDto>();
    }
}
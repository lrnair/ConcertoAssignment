namespace DotNetInterview.API.DTO;

public record ItemDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public ICollection<VariationDto> Variations { get; set; } = new List<VariationDto>();
}

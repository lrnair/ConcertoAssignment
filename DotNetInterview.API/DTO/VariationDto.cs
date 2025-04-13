namespace DotNetInterview.API.DTO;

public record VariationDto
{
    public Guid? Id { get; set; }   // nullable for adding new Variations
    public Guid? ItemId { get; set; }   // nullable for adding new Variations
    public string Size { get; set; }    
    public int Quantity { get; set; }
}

namespace DotNetInterview.API.Domain;

public record Variation
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string Size { get; set; }
    public int Quantity { get; set; }
}

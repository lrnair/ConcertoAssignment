namespace DotNetInterview.API.DTO;

public record GetAllItemsDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; }
	public string Name { get; set; }
	public decimal Price { get; set; }
	public decimal HighestDiscount { get; set; }
	public decimal PriceAfterDiscount { get; set; }
	public int StockQuantity {  get; set; }
	public string StockStatus { get; set; }
}

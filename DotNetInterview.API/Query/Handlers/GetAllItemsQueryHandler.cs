using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetInterview.API.DTO;

namespace DotNetInterview.API.Query
{
    public class GetAllItemsHandler : IRequestHandler<GetAllItemsQuery, IEnumerable<GetAllItemsDto>>
    {
        private readonly DataContext _context;

        public GetAllItemsHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GetAllItemsDto>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken)
        {
            var items = await _context.Items
                                 .Include(i => i.Variations)
                                 .ToListAsync(cancellationToken);

            var result = items.Select(item =>
            {
                var stockQuantity = item.Variations.Sum(v => v.Quantity);
                var stockStatus = stockQuantity > 0 ? "In Stock": "Sold Out";
                var highestDiscount = CalculateHighestDiscount(stockQuantity);
                var priceAfterDiscount = item.Price * (1 - highestDiscount);    // price after applying highest discount

                return new GetAllItemsDto
                {
                    Id = item.Id,
                    Reference = item.Reference,
                    Name = item.Name,
                    Price = item.Price,
                    HighestDiscount = highestDiscount,
                    PriceAfterDiscount = priceAfterDiscount,
                    StockQuantity = stockQuantity,
                    StockStatus = stockStatus
                };
            });

            return result;
        }

        // determines highest discount available for an item
        // ***
        // When the quantity of stock for an item is greater than 5, the price should be discounted by 10%
        // When the quantity of stock for an item is greater than 10, the price should be discounted by 20%
        // Every Monday between 12pm and 5pm, all items are discounted by 50%
        // Only a single discount should be applied to an item at any time, the highest discount percentage
        public decimal CalculateHighestDiscount(int stockQuantity)
        {
            decimal discount = 0;
            DateTime utcTime = DateTime.UtcNow;
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.Local);

            if (stockQuantity > 10)
                discount = 0.20m;
            else if (stockQuantity > 5)
                discount = 0.10m;

            if (localTime.DayOfWeek == DayOfWeek.Monday &&
                localTime.TimeOfDay >= TimeSpan.FromHours(12) &&
                localTime.TimeOfDay <= TimeSpan.FromHours(17))
            {
                discount = Math.Max(discount, 0.50m); // highest discount percentage selected based on stock quantity and time (12-5pm) on a Monday
            }

            return discount;
        }
    }
}
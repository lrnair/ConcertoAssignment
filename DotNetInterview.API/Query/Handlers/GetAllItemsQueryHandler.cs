using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetInterview.API.Domain;

namespace DotNetInterview.API.Query
{
    public class GetAllItemsHandler : IRequestHandler<GetAllItemsQuery, IEnumerable<Item>>
    {
        private readonly DataContext _context;

        public GetAllItemsHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Item>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken)
        {
            var items = await _context.Items
                                 .Include(i => i.Variations)
                                 .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.PriceAfterDiscount = CalculatePriceAfterDiscount(item);
            }

            return items;
        }
    }

    // determines highest discount available for an item and returns price after applying highest discount
    // When the quantity of stock for an item is greater than 5, the price should be discounted by 10%
    // When the quantity of stock for an item is greater than 10, the price should be discounted by 20%
    // Every Monday between 12pm and 5pm, all items are discounted by 50%
    // Only a single discount should be applied to an item at any time, the highest discount percentage
    public decimal CalculatePriceAfterDiscount(Item item)
        {
            decimal discount = 0;
            DateTime utcTime = DateTime.UtcNow;
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.Local);
            int stockQuantity = item.Variations.Sum(v => v.Quantity);

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
            
            item.HighestDiscount = discount;

            return item.OriginalPrice * (1 - discount);
        }
    }
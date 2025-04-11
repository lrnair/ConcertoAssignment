using DotNetInterview.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace DotNetInterview.API;

internal static class SeedData
{
    public static void Load(ModelBuilder modelBuilder)
    {
        var item1 = new Item
        {
            Id = Guid.NewGuid(),
            Reference = "A123",
            Name = "Shorts",
            Price = 35,
        };
        var item1Variation1 = new Variation
        {
            Id = Guid.NewGuid(),
            ItemId = item1.Id,
            Size = "Small",
            Quantity = 7
        };
        var item1Variation2 = new Variation
        {
            Id = Guid.NewGuid(),
            ItemId = item1.Id,
            Size = "Medium",
            Quantity = 0
        };
        var item1Variation3 = new Variation
        {
            Id = Guid.NewGuid(),
            ItemId = item1.Id,
            Size = "Large",
            Quantity = 3
        };

        var item2 = new Item
        {
            Id = Guid.NewGuid(),
            Reference = "B456",
            Name = "Tie",
            Price = 15,
        };

        var item3 = new Item
        {
            Id = Guid.NewGuid(),
            Reference = "C789",
            Name = "Shoes",
            Price = 70,
        };
        var item3Variation1 = new Variation
        {
            Id = Guid.NewGuid(),
            ItemId = item3.Id,
            Size = "9",
            Quantity = 7
        };
        var item3Variation2 = new Variation
        {
            Id = Guid.NewGuid(),
            ItemId = item3.Id,
            Size = "10",
            Quantity = 8
        };

        modelBuilder.Entity<Item>()
            .HasData(item1, item2, item3);

        modelBuilder.Entity<Variation>()
            .HasData(item1Variation1, item1Variation2, item1Variation3, item3Variation1, item3Variation2);
    }
}
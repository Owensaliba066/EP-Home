using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace DataAccess.Repositories
{
    public class ItemsDbRepository : IItemsRepository
    {
        private readonly AppDbContext _db;

        public ItemsDbRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<IItemValidating>> GetAsync()
        {
            var restaurants = await _db.Restaurants.ToListAsync();
            var menuItems = await _db.MenuItems.ToListAsync();

            var items = new List<IItemValidating>();
            items.AddRange(restaurants);
            items.AddRange(menuItems);

            return items;
        }

        public async Task SaveAsync(List<IItemValidating> items)
        {
            // Take only Restaurant items from the list
            var restaurants = items.OfType<Restaurant>().ToList();

            if (restaurants.Any())
            {
                // Mark all of them as Pending if Status is empty
                foreach (var restaurant in restaurants)
                {
                    if (string.IsNullOrWhiteSpace(restaurant.Status))
                    {
                        restaurant.Status = "Pending";
                    }
                }

                // Add them all as NEW rows
                _db.Restaurants.AddRange(restaurants);
                await _db.SaveChangesAsync();
            }
        }

        public Task ClearAsync()
        {
            // Nothing to clear at DB level; method exists for interface symmetry
            return Task.CompletedTask;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Returns only APPROVED restaurants and menu items for the public catalog.
        /// </summary>
        public async Task<List<IItemValidating>> GetAsync()
        {
            var restaurants = await _db.Restaurants
                .Where(r => r.Status == "Approved")
                .ToListAsync();

            var menuItems = await _db.MenuItems
                .Where(m => m.Status == "Approved")
                .ToListAsync();

            var items = new List<IItemValidating>();
            items.AddRange(restaurants);
            items.AddRange(menuItems);

            return items;
        }

        /// <summary>
        /// Saves NEW items (restaurants + menu items) with Status defaulting to Pending.
        /// Used by BulkImport Commit.
        /// </summary>
        public async Task SaveAsync(List<IItemValidating> items)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }

            var restaurants = items.OfType<Restaurant>().ToList();
            var menuItems = items.OfType<MenuItem>().ToList();

            foreach (var restaurant in restaurants)
            {
                if (string.IsNullOrWhiteSpace(restaurant.Status))
                {
                    restaurant.Status = "Pending";
                }
            }

            foreach (var menuItem in menuItems)
            {
                if (string.IsNullOrWhiteSpace(menuItem.Status))
                {
                    menuItem.Status = "Pending";
                }
            }

            if (restaurants.Any())
            {
                _db.Restaurants.AddRange(restaurants);
            }

            if (menuItems.Any())
            {
                _db.MenuItems.AddRange(menuItems);
            }

            if (restaurants.Any() || menuItems.Any())
            {
                await _db.SaveChangesAsync();
            }
        }

        public Task ClearAsync()
        {
            // Nothing to clear at DB level; method exists for interface symmetry
            return Task.CompletedTask;
        }


        /// <summary>
        /// Restaurants with Status == "Pending" (for Site Admin verification).
        /// </summary>
        public async Task<List<Restaurant>> GetPendingRestaurantsAsync()
        {
            return await _db.Restaurants
                .Where(r => r.Status == "Pending")
                .ToListAsync();
        }

        /// <summary>
        /// Restaurants owned by given email (for restaurant owner verification).
        /// </summary>
        public async Task<List<Restaurant>> GetOwnedRestaurantsAsync(string ownerEmail)
        {
            return await _db.Restaurants
                .Where(r => r.OwnerEmailAddress == ownerEmail)
                .ToListAsync();
        }

        /// <summary>
        /// Pending menu items belonging to a particular restaurant.
        /// </summary>
        public async Task<List<MenuItem>> GetPendingMenuItemsForRestaurantAsync(int restaurantId)
        {
            return await _db.MenuItems
                .Where(m => m.RestaurantId == restaurantId && m.Status == "Pending")
                .ToListAsync();
        }

        /// <summary>
        /// Approve multiple restaurants by id (sets Status = "Approved").
        /// </summary>
        public async Task ApproveRestaurantsAsync(IEnumerable<int> restaurantIds)
        {
            var ids = restaurantIds?.ToList() ?? new List<int>();
            if (!ids.Any())
            {
                return;
            }

            var restaurants = await _db.Restaurants
                .Where(r => ids.Contains(r.Id))
                .ToListAsync();

            foreach (var restaurant in restaurants)
            {
                restaurant.Status = "Approved";
            }

            if (restaurants.Any())
            {
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Approve multiple menu items by id (sets Status = "Approved").
        /// </summary>
        public async Task ApproveMenuItemsAsync(IEnumerable<Guid> menuItemIds)
        {
            var ids = menuItemIds?.ToList() ?? new List<Guid>();
            if (!ids.Any())
            {
                return;
            }

            var menuItems = await _db.MenuItems
                .Where(m => ids.Contains(m.Id))
                .ToListAsync();

            foreach (var menuItem in menuItems)
            {
                menuItem.Status = "Approved";
            }

            if (menuItems.Any())
            {
                await _db.SaveChangesAsync();
            }
        }
    }
}

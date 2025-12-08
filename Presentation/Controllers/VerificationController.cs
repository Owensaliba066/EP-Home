using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Repositories;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Filters;

namespace Presentation.Controllers
{
    [Authorize]
    public class VerificationController : Controller
    {
        private readonly ItemsDbRepository _dbRepository;

        // Keep this consistent with Restaurant.GetValidators()
        private const string SiteAdminEmail = "siteadmin@example.com";

        public VerificationController(ItemsDbRepository dbRepository)
        {
            _dbRepository = dbRepository;
        }

        /// <summary>
        /// If Site Admin: show pending restaurants with checkboxes.
        /// If restaurant owner: show owned restaurants OR pending menu items for a selected restaurant.
        /// </summary>
        public async Task<IActionResult> Index(int? restaurantId)
        {
            var userEmail = User.Identity?.Name ?? string.Empty;

            var isAdmin = string.Equals(userEmail, SiteAdminEmail, StringComparison.OrdinalIgnoreCase);

            if (isAdmin)
            {
                // Site admin – show all pending restaurants.
                var pendingRestaurants = await _dbRepository.GetPendingRestaurantsAsync();
                return View("PendingRestaurants", pendingRestaurants);
            }

            // Normal user (restaurant owner)
            if (restaurantId == null)
            {
                // Step 1: show owned restaurants (click to drill into menu items)
                var ownedRestaurants = await _dbRepository.GetOwnedRestaurantsAsync(userEmail);
                return View("OwnedRestaurants", ownedRestaurants);
            }
            else
            {
                // Step 2: for a chosen restaurant, show its pending menu items
                var pendingMenuItems = await _dbRepository.GetPendingMenuItemsForRestaurantAsync(restaurantId.Value);
                ViewBag.RestaurantId = restaurantId.Value;
                return View("PendingMenuItems", pendingMenuItems);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(RestaurantApprovalFilter))]
        public async Task<IActionResult> ApproveRestaurants(int[] selectedRestaurantIds)
        {
            if (selectedRestaurantIds == null || selectedRestaurantIds.Length == 0)
            {
                TempData["Message"] = "No restaurants selected.";
                return RedirectToAction("Index");
            }

            await _dbRepository.ApproveRestaurantsAsync(selectedRestaurantIds);
            TempData["Message"] = "Selected restaurants have been approved.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(MenuItemApprovalFilter))]
        public async Task<IActionResult> ApproveMenuItems(int restaurantId, int[] selectedMenuItemIds)
        {
            if (selectedMenuItemIds == null || selectedMenuItemIds.Length == 0)
            {
                TempData["Message"] = "No menu items selected.";
                return RedirectToAction("Index", new { restaurantId });
            }

            await _dbRepository.ApproveMenuItemsAsync(selectedMenuItemIds);
            TempData["Message"] = "Selected menu items have been approved.";
            return RedirectToAction("Index", new { restaurantId });
        }
    }
}

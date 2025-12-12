using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Filters
{
    //Action filter that checks whether the current user is allowed to approve or reject the selected menu items.
    public class MenuItemApprovalFilter : IAsyncActionFilter
    {
        private readonly AppDbContext _db;

        public MenuItemApprovalFilter(AppDbContext db)
        {
            _db = db;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            // User must be logged in to perform approval actions
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new ForbidResult();
                return;
            }

            var userEmail = user.Identity.Name ?? string.Empty;

            if (!context.ActionArguments.TryGetValue("selectedMenuItemIds", out var value) ||
                value is not int[] ids ||
                ids.Length == 0)
            {
                await next();
                return;
            }

            var menuItems = await _db.MenuItems
                .Include(m => m.Restaurant)
                .Where(m => ids.Contains(m.Id))
                .ToListAsync();

            foreach (var menuItem in menuItems)
            {
                var item = (IItemValidating)menuItem;
                var validators = item.GetValidators();

                // If the current user is not one of the validators, block the action.
                if (!validators.Any(v =>
                        string.Equals(v, userEmail, StringComparison.OrdinalIgnoreCase)))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            await next();
        }
    }
}

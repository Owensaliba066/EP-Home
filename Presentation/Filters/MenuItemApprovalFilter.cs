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

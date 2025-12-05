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
    public class RestaurantApprovalFilter : IAsyncActionFilter
    {
        private readonly AppDbContext _db;

        public RestaurantApprovalFilter(AppDbContext db)
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

            // Expecting the action to have an int[] parameter named "selectedRestaurantIds"
            if (!context.ActionArguments.TryGetValue("selectedRestaurantIds", out var value) ||
                value is not int[] ids ||
                ids.Length == 0)
            {
                // Nothing selected – just proceed; action can handle validation.
                await next();
                return;
            }

            var restaurants = await _db.Restaurants
                .Where(r => ids.Contains(r.Id))
                .ToListAsync();

            foreach (var restaurant in restaurants)
            {
                var item = (IItemValidating)restaurant;
                var validators = item.GetValidators();

                if (!validators.Any(v =>
                        string.Equals(v, userEmail, StringComparison.OrdinalIgnoreCase)))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            // All good, proceed with the action
            await next();
        }
    }
}

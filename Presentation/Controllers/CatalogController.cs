using Microsoft.AspNetCore.Mvc;
using DataAccess.Context;
using Domain.Interfaces;
using System.Linq;

namespace Presentation.Controllers
{
    public class CatalogController : Controller
    {
        private readonly AppDbContext _db;

        public CatalogController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            // Load restaurants + menu items
            var restaurants = _db.Restaurants.ToList();
            var menuItems = _db.MenuItems.ToList();

            // Combine into a single polymorphic list
            List<IItemValidating> items = new List<IItemValidating>();
            items.AddRange(restaurants);
            items.AddRange(menuItems);

            return View(items);
        }
    }
}

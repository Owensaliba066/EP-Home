using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Repositories;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ItemsDbRepository _dbRepository;

        public CatalogController(ItemsDbRepository dbRepository)
        {
            _dbRepository = dbRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Only approved items are returned by ItemsDbRepository.GetAsync()
            List<IItemValidating> items = await _dbRepository.GetAsync();
            return View(items);
        }
    }
}

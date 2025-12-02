using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Repositories;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.Factory;

namespace Presentation.Controllers
{
    public class BulkImportController : Controller
    {
        private readonly ImportItemFactory _factory;
        private readonly ItemsInMemoryRepository _inMemoryRepository;
        private readonly ItemsDbRepository _dbRepository;

        public BulkImportController(
            ImportItemFactory factory,
            ItemsInMemoryRepository inMemoryRepository,
            ItemsDbRepository dbRepository)
        {
            _factory = factory;
            _inMemoryRepository = inMemoryRepository;
            _dbRepository = dbRepository;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile jsonFile)
        {
            if (jsonFile == null || jsonFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please choose a JSON file to upload.");
                return View();
            }

            string content;
            using (var stream = jsonFile.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                content = await reader.ReadToEndAsync();
            }

            // Parse JSON into domain objects
            List<IItemValidating> items = _factory.ParseItems(content);

            // Save items temporarily in memory so Commit can read them
            await _inMemoryRepository.ClearAsync();
            await _inMemoryRepository.SaveAsync(items);

            // Show preview
            return View("Preview", items);
        }

        [HttpPost]
        public async Task<IActionResult> Commit()
        {
            var items = await _inMemoryRepository.GetAsync();

            if (items == null || !items.Any())
            {
                TempData["Message"] = "No items to commit. Please upload a JSON file first.";
                return RedirectToAction("Upload");
            }

            await _dbRepository.SaveAsync(items);
            await _inMemoryRepository.ClearAsync();

            TempData["Message"] = $"Committed {items.Count} item(s) to the database.";
            return RedirectToAction("Upload");
        }
    }
}

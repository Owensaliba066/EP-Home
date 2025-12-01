using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.Factory;
using Domain.Interfaces;
using System.Collections.Generic;

namespace Presentation.Controllers
{
    public class BulkImportController : Controller
    {
        private readonly ImportItemFactory _factory;

        public BulkImportController(ImportItemFactory factory)
        {
            _factory = factory;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            // Later we will return a view with a file upload form.
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

            // Use the factory to parse the JSON into domain objects
            List<IItemValidating> items = _factory.ParseItems(content);

            // For now, just pass the list to a Preview view (we'll create it next).
            return View("Preview", items);
        }

        // We’ll flesh this out later when we implement saving to DB.
        [HttpPost]
        public IActionResult Commit()
        {
            TempData["Message"] = "Commit not implemented yet.";
            return RedirectToAction("Upload");
        }
    }
}

using DataAccess.Repositories;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.Factory;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    public class BulkImportController : Controller
    {
        private readonly ImportItemFactory _factory;
        private readonly ItemsInMemoryRepository _inMemoryRepository;
        private readonly ItemsDbRepository _dbRepository;
        private readonly IWebHostEnvironment _environment;

        public BulkImportController(
            ImportItemFactory factory,
            ItemsInMemoryRepository inMemoryRepository,
            ItemsDbRepository dbRepository,
            IWebHostEnvironment environment)
        {
            _factory = factory;
            _inMemoryRepository = inMemoryRepository;
            _dbRepository = dbRepository;
            _environment = environment;
        }

        // UPLOAD JSON (GET)
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        // UPLOAD JSON (POST)
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

            // Save parsed items in memory for Commit
            await _inMemoryRepository.ClearAsync();
            await _inMemoryRepository.SaveAsync(items);

            return View("Preview", items);
        }

        // DOWNLOAD IMAGES TEMPLATE ZIP
        [HttpGet]
        public async Task<IActionResult> DownloadImagesTemplate()
        {
            var items = await _inMemoryRepository.GetAsync();

            if (items == null || !items.Any())
            {
                TempData["Message"] = "No items available for image template. Please upload a JSON file first.";
                return RedirectToAction("Upload");
            }

            using var ms = new MemoryStream();

            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                int index = 1;

                foreach (var item in items)
                {
                    string folderName;

                    if (item is Restaurant)
                        folderName = $"restaurant-{index}";
                    else if (item is MenuItem)
                        folderName = $"menuitem-{index}";
                    else
                        folderName = $"item-{index}";

                    string entryPath = $"{folderName}/default.jpg";

                    var entry = archive.CreateEntry(entryPath, CompressionLevel.Fastest);

                    using var entryStream = entry.Open();
                    byte[] placeholderBytes = Encoding.UTF8.GetBytes("placeholder image");
                    entryStream.Write(placeholderBytes, 0, placeholderBytes.Length);

                    index++;
                }
            }

            ms.Position = 0;

            return File(ms.ToArray(), "application/zip", "images-template.zip");
        }

        // COMMIT JSON + OPTIONAL ZIP UPLOAD
        [HttpPost]
        public async Task<IActionResult> Commit(IFormFile imagesZip)
        {
            var items = await _inMemoryRepository.GetAsync();

            if (items == null || !items.Any())
            {
                TempData["Message"] = "No items to commit. Please upload a JSON file first.";
                return RedirectToAction("Upload");
            }

            // Extract images ZIP into wwwroot/images/import
            if (imagesZip != null && imagesZip.Length > 0)
            {
                string webRoot = _environment.WebRootPath;
                string imagesRoot = Path.Combine(webRoot, "images", "import");

                Directory.CreateDirectory(imagesRoot);

                using var zipStream = imagesZip.OpenReadStream();
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    string destinationPath = Path.Combine(imagesRoot, entry.FullName);

                    string? dir = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);

                    using var entryStream = entry.Open();
                    using var fileStream = System.IO.File.Create(destinationPath);
                    await entryStream.CopyToAsync(fileStream);
                }
            }

            // Save items to database
            await _dbRepository.SaveAsync(items);

            // Clear in-memory store after commit
            await _inMemoryRepository.ClearAsync();

            TempData["Message"] = $"Committed {items.Count} item(s) to the database.";
            return RedirectToAction("Upload");
        }
    }
}

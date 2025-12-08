using System;
using System.Collections.Generic;
using System.Text.Json;
using Domain.Interfaces;
using Domain.Models;

namespace Presentation.Factory
{
    /// <summary>
    /// Responsible for reading JSON data and creating the correct
    /// domain objects (Restaurant, MenuItem, etc.) that implement
    /// IItemValidating.
    /// </summary>
    public class ImportItemFactory
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public ImportItemFactory()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Parses a JSON string containing an array of items.
        /// Each item must have a "type" property such as "restaurant" or "menuItem".
        /// Returns a list of domain objects that implement IItemValidating.
        /// </summary>
        public List<IItemValidating> ParseItems(string jsonContent)
        {
            var result = new List<IItemValidating>();

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return result;
            }

            JsonDocument document;

            try
            {
                document = JsonDocument.Parse(jsonContent);
            }
            catch (Exception ex)
            {
                // If this throws, the JSON itself is invalid.
                throw new ApplicationException("Invalid JSON: could not parse content.", ex);
            }

            using (document)
            {
                var root = document.RootElement;

                // If someone wrapped it like { "items": [ ... ] }, support that too:
                if (root.ValueKind == JsonValueKind.Object &&
                    root.TryGetProperty("items", out var itemsElement) &&
                    itemsElement.ValueKind == JsonValueKind.Array)
                {
                    root = itemsElement;
                }

                if (root.ValueKind != JsonValueKind.Array)
                {
                    throw new ApplicationException(
                        $"Expected JSON array at the root (or an 'items' array), but got {root.ValueKind}.");
                }

                foreach (var element in root.EnumerateArray())
                {
                    if (!element.TryGetProperty("type", out var typeProp))
                    {
                        // no type -> skip
                        continue;
                    }

                    var type = typeProp.GetString()?.ToLowerInvariant();

                    var rawItemJson = element.GetRawText();

                    switch (type)
                    {
                        case "restaurant":
                            {
                                var restaurant = JsonSerializer.Deserialize<Restaurant>(rawItemJson, _jsonOptions);
                                if (restaurant == null)
                                {
                                    throw new ApplicationException(
                                        "Failed to deserialize a restaurant item: " + rawItemJson);
                                }
                                result.Add(restaurant);
                                break;
                            }

                        case "menuitem":
                        case "menu_item":
                        case "menu-item":
                            {
                                var menuItem = JsonSerializer.Deserialize<MenuItem>(rawItemJson, _jsonOptions);
                                if (menuItem == null)
                                {
                                    throw new ApplicationException(
                                        "Failed to deserialize a menu item: " + rawItemJson);
                                }
                                result.Add(menuItem);
                                break;
                            }

                        default:
                            // Unknown type -> ignore for now
                            break;
                    }
                }
            }

            return result;
        }
    }
}

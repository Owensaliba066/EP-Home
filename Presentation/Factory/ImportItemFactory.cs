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

            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Array)
            {
                // We expect an array at the root – if not, just return empty for now.
                return result;
            }

            foreach (var element in root.EnumerateArray())
            {
                if (!element.TryGetProperty("type", out var typeProp))
                {
                    // Skip items that don't declare a type
                    continue;
                }

                var type = typeProp.GetString()?.ToLowerInvariant();

                try
                {
                    switch (type)
                    {
                        case "restaurant":
                            var restaurant = element.Deserialize<Restaurant>(_jsonOptions);
                            if (restaurant != null)
                            {
                                result.Add(restaurant);
                            }
                            break;

                        case "menuitem":
                        case "menu_item":
                        case "menu-item":
                            var menuItem = element.Deserialize<MenuItem>(_jsonOptions);
                            if (menuItem != null)
                            {
                                result.Add(menuItem);
                            }
                            break;

                        default:
                            // Unknown type – ignore for now
                            break;
                    }
                }
                catch (Exception)
                {
                    // If one item fails to deserialize, just skip it.
                    // We keep this silent for now; later we can log it if needed.
                }
            }

            return result;
        }
    }
}

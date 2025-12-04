using System.Collections.Generic;

namespace Domain.Interfaces
{
    /// <summary>
    /// Represents an item that can be validated/approved by one or more users.
    /// </summary>
    public interface IItemValidating
    {
        /// <summary>
        /// Returns the list of email addresses that are allowed to approve this item.
        /// </summary>
        List<string> GetValidators();

        /// <summary>
        /// Returns the name of the partial view used to render this item
        /// in the catalog (e.g. "_RestaurantCard" or "_MenuItemRow").
        /// </summary>
        string GetCardPartial();
    }
}

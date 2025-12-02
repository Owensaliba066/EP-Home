using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IItemsRepository
    {
        Task SaveAsync(List<IItemValidating> items);
        Task<List<IItemValidating>> GetAsync();
        Task ClearAsync();
    }
}

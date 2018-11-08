using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using DailyPrayer.Models;

namespace DailyPrayer.Services
{
    public interface IContactsService
    {
        Task<IEnumerable<MobileContact>> GetAllContactsAsync();
        List<MobileContact> FindContacts(string searchInContactsString);
        Task<bool> GetPermissionAsync();
    }
}

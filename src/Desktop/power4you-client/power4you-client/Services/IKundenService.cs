using System.Collections.Generic;
using System.Threading.Tasks;
using power4you_client.Models;

namespace power4you_client.Services
{
    public interface IKundenService
    {
        Task<List<Kunde>> GetAllKundenAsync();
        Task<Kunde> GetKundeByIdAsync(int id);
        Task<Kunde> AddKundeAsync(Kunde kunde);
        Task<Kunde> UpdateKundeAsync(Kunde kunde);
        Task DeleteKundeAsync(int id);
    }
}
using NRI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static NRI.Classes.User;

namespace NRI.Services
{
    public interface IGameSystemService
    {
        Task<IEnumerable<GameSystem>> GetAllGameSystemsAsync();
        Task<GameSystem> GetGameSystemByIdAsync(int id);
        Task<bool> AddGameSystemAsync(GameSystem system);
        Task<bool> UpdateGameSystemAsync(GameSystem system);
        Task<bool> DeleteGameSystemAsync(int id);

    }
}
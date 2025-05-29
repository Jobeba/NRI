using Microsoft.EntityFrameworkCore;
using NRI.Data;
using NRI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NRI.Classes.User;

namespace NRI.Services
{
    public class GameSystemService : IGameSystemService
    {
        private readonly AppDbContext _context;

        public GameSystemService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GameSystem>> GetAllGameSystemsAsync()
        {
            using var context = new AppDbContext();
            return await context.GameSystems.ToListAsync();
        }

        public async Task<GameSystem> GetGameSystemByIdAsync(int id)
        {
            return await _context.GameSystems.FindAsync(id);
        }

        public async Task<bool> AddGameSystemAsync(GameSystem system)
        {
            await _context.GameSystems.AddAsync(system);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateGameSystemAsync(GameSystem system)
        {
            _context.GameSystems.Update(system);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteGameSystemAsync(int id)
        {
            var system = await GetGameSystemByIdAsync(id);
            if (system == null) return false;

            _context.GameSystems.Remove(system);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}